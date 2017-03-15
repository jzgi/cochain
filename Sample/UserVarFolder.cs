﻿using Greatbone.Core;

namespace Greatbone.Sample
{
    [Ui("用户")]
    public class UserVarFolder : Folder, IVar
    {
        public UserVarFolder(FolderContext fc) : base(fc)
        {
        }

        public void @default(ActionContext ac, int page)
        {
            string userid = ac[this];

            using (var dc = ac.NewDbContext())
            {
                if (dc.Query("SELECT * FROM orders WHERE buyerid = @1 ORDER BY id LIMIT 20 OFFSET @2", p => p.Set(userid).Set(page * 20)))
                {
                    ac.GiveFolderPage(null, 200, dc.ToList<Order>());
                }
                else
                {
                    ac.Give(204); // no content
                }
            }
        }

        [Ui]
        [State]
        public void cancel(ActionContext ac)
        {

        }

        [Ui("基本资料", Modal = 1)]
        public void profile(ActionContext ac)
        {
            string userid = ac[this];

            using (var dc = ac.NewDbContext())
            {
                if (dc.Query1("SELECT * FROM users WHERE id = @1", p => p.Set(userid)))
                {
                    ac.GiveModalForm(200, dc.ToObject<User>());
                }
                else
                {
                    ac.Give(404); // not found
                }
            }
        }

        [Ui("设置密码", Modal = 1)]
        public void pass(ActionContext ac)
        {

        }
    }
}