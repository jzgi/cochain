﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Greatbone.Core
{
    ///
    /// A work is a virtual web folder that contains a single or collection of resources along with operations on it or them.
    /// A work can contain child/sub works.
    ///
    public abstract class Work : Nodule
    {
        internal static readonly AuthorizeException AuthorizeEx = new AuthorizeException();

        // max nesting levels
        const int MaxNesting = 6;

        // underlying file directory name
        const string _VAR_ = "VAR";

        // state-passing
        readonly WorkContext wc;

        // declared actions 
        readonly Roll<ActionInfo> actions;

        // the default action, can be null
        readonly ActionInfo @default;

        // the goto action, can be null
        readonly ActionInfo @goto;

        // actions with Ui attribute
        readonly ActionInfo[] uiactions;

        // subworks, if any
        internal Roll<Work> subworks;

        // the attached variable-key subwork, if any
        internal Work varwork;

        // to obtain a string key from a data object.
        Func<IData, string> varkey;

        protected Work(WorkContext wc) : base(wc.Name, null)
        {
            this.wc = wc;

            // gather actions
            actions = new Roll<ActionInfo>(32);
            Type typ = GetType();
            foreach (MethodInfo mi in typ.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                // verify the return type
                Type ret = mi.ReturnType;
                bool async;
                if (ret == typeof(Task)) async = true;
                else if (ret == typeof(void)) async = false;
                else continue;

                ParameterInfo[] pis = mi.GetParameters();
                ActionInfo ai = null;
                if (pis.Length == 1 && pis[0].ParameterType == typeof(ActionContext))
                {
                    ai = new ActionInfo(this, mi, async, false);
                }
                else if (pis.Length == 2 && pis[0].ParameterType == typeof(ActionContext) && pis[1].ParameterType == typeof(int))
                {
                    ai = new ActionInfo(this, mi, async, true);
                }
                else continue;

                actions.Add(ai);
                if (ai.Name.Equals("default")) { @default = ai; }
                if (ai.Name.Equals("goto")) { @goto = ai; }
            }

            // gather ui actions
            List<ActionInfo> uias = null;
            for (int i = 0; i < actions.Count; i++)
            {
                ActionInfo a = actions[i];
                if (a.HasUi)
                {
                    if (uias == null) uias = new List<ActionInfo>();
                    uias.Add(a);
                }
            }
            uiactions = uias?.ToArray();
        }

        ///
        /// Create a subwork.
        ///
        public W Create<W>(string name, object attachment = null) where W : Work
        {
            if (Level >= MaxNesting)
            {
                throw new ServiceException("allowed work nesting " + MaxNesting);
            }

            if (subworks == null)
            {
                subworks = new Roll<Work>(16);
            }
            // create instance by reflection
            Type typ = typeof(W);
            ConstructorInfo ci = typ.GetConstructor(new[] { typeof(WorkContext) });
            if (ci == null)
            {
                throw new ServiceException(typ + " missing WorkContext");
            }
            WorkContext wc = new WorkContext(name)
            {
                Attachment = attachment,
                Parent = this,
                Level = Level + 1,
                Directory = (Parent == null) ? name : Path.Combine(Parent.Directory, name),
                Service = Service
            };
            W work = (W)ci.Invoke(new object[] { wc });
            Subworks.Add(work);

            return work;
        }

        ///
        /// Create a variable work.
        ///
        public W CreateVar<W>(Func<IData, string> key = null, object attachment = null) where W : Work
        {
            if (Level >= MaxNesting)
            {
                throw new ServiceException("allowed work nesting " + MaxNesting);
            }

            // create instance
            Type typ = typeof(W);
            ConstructorInfo ci = typ.GetConstructor(new[] { typeof(WorkContext) });
            if (ci == null)
            {
                throw new ServiceException(typ + " missing WorkContext");
            }
            WorkContext wc = new WorkContext(_VAR_)
            {
                Attachment = attachment,
                Parent = this,
                IsVar = true,
                Level = Level + 1,
                Directory = (Parent == null) ? _VAR_ : Path.Combine(Parent.Directory, _VAR_),
                Service = Service
            };
            W work = (W)ci.Invoke(new object[] { wc });
            varkey = key;
            varwork = work;
            return work;
        }

        public Roll<ActionInfo> Actions => actions;

        public ActionInfo[] UiActions => uiactions;

        public bool HasDefault => @default != null;

        public bool HasGoto => @goto != null;

        public Roll<Work> Subworks => subworks;

        public Work Varwork => varwork;

        public Func<IData, string> Varkey => varkey;

        public string Directory => wc.Directory;

        public Work Parent => wc.Parent;

        public bool IsVar => wc.IsVar;

        public int Level => wc.Level;

        public override Service Service => wc.Service;

        internal void Describe(XmlContent cont)
        {
            cont.ELEM(Name,
            delegate
            {
                for (int i = 0; i < Actions.Count; i++)
                {
                    ActionInfo act = Actions[i];
                    cont.Put(act.Name, "");
                }
            },
            delegate
            {
                if (this.Subworks != null)
                {
                    for (int i = 0; i < this.Subworks.Count; i++)
                    {
                        Work work = this.Subworks[i];
                        work.Describe(cont);
                    }
                }
                if (varwork != null)
                {
                    varwork.Describe(cont);
                }
            });
        }

        public ActionInfo GetAction(string method)
        {
            if (string.IsNullOrEmpty(method))
            {
                return @default;
            }
            return actions[method];
        }

        internal Work Resolve(ref string relative, ActionContext ac, ref bool recover)
        {
            int slash = relative.IndexOf('/');
            if (slash == -1) { return this; }

            // seek subworks/varwork
            //
            string key = relative.Substring(0, slash);
            relative = relative.Substring(slash + 1); // adjust relative
            Work work;
            if (subworks != null && subworks.TryGet(key, out work)) // if child
            {
                ac.Chain(key, work);
                return work.Resolve(ref relative, ac, ref recover);
            }
            if (varwork != null) // if variable-key sub
            {
                if (key.Length == 0 && varkey != null) // resolve varkey
                {
                    if (ac.Principal == null) throw AuthorizeEx;
                    if ((key = varkey(ac.Principal)) == null)
                    {
                        if (@goto != null) { recover = true; }
                        return null;
                    }
                }
                ac.Chain(key, varwork);
                return varwork.Resolve(ref relative, ac, ref recover);
            }
            return null;
        }

        internal async Task HandleAsync(string rsc, ActionContext ac)
        {
            ac.Work = this;

            // access check 
            if (!DoAuthorize(ac)) throw AuthorizeEx;

            // pre-
            BeforeAttribute bef = Before;
            if (bef != null) { if (bef.IsAsync) await bef.DoAsync(ac); else bef.Do(ac); }

            int dot = rsc.LastIndexOf('.');
            if (dot != -1) // file
            {
                // try in cache 

                DoFile(rsc, rsc.Substring(dot), ac);
            }
            else // action
            {
                string name = rsc;
                int subscpt = 0;
                int dash = rsc.LastIndexOf('-');
                if (dash != -1)
                {
                    name = rsc.Substring(0, dash);
                    ac.Subscript = subscpt = rsc.Substring(dash + 1).ToInt();
                }
                ActionInfo act = string.IsNullOrEmpty(name) ? @default : GetAction(name);
                if (act == null)
                {
                    ac.Give(404); // not found
                    return;
                }

                ac.Doer = act;

                // access check
                if (!act.DoAuthorize(ac)) throw AuthorizeEx;

                // try in cache

                BeforeAttribute actbef = act.Before;
                if (actbef != null) { if (actbef.IsAsync) await actbef.DoAsync(ac); else actbef.Do(ac); }

                // method invocation
                if (act.IsAsync)
                {
                    await act.DoAsync(ac, subscpt); // invoke action method
                }
                else
                {
                    act.Do(ac, subscpt);
                }

                AfterAttribute actaft = act.After;
                if (actaft != null) { if (actaft.IsAsync) await actaft.DoAsync(ac); else actaft.Do(ac); }

                ac.Doer = null;
            }

            // post-
            AfterAttribute aft = After;
            if (aft != null) { if (aft.IsAsync) await aft.DoAsync(ac); else aft.Do(ac); }

            ac.Work = null;
        }

        public void DoFile(string filename, ActionContext ac)
        {
            int dot = filename.LastIndexOf('.');
            DoFile(filename, filename.Substring(dot), ac);
        }

        void DoFile(string filename, string ext, ActionContext ac)
        {
            if (filename.StartsWith("$")) // private resource
            {
                ac.Give(403); // forbidden
                return;
            }

            string ctyp;
            if (!StaticContent.TryGetType(ext, out ctyp))
            {
                ac.Give(415); // unsupported media type
                return;
            }

            string path = Path.Combine(Directory, filename);
            if (!File.Exists(path))
            {
                ac.Give(404); // not found
                return;
            }

            DateTime modified = File.GetLastWriteTime(path);
            DateTime? since = ac.HeaderDateTime("If-Modified-Since");
            if (since != null && modified <= since)
            {
                ac.Give(304); // not modified
                return;
            }

            // load file content
            byte[] bytes = File.ReadAllBytes(path);
            StaticContent cont = new StaticContent(true, bytes, bytes.Length)
            {
                Name = filename,
                Type = ctyp,
                Modified = modified
            };
            ac.Give(200, cont, true, 3600 * 12);
        }
    }
}