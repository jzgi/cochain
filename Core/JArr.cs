using System;

namespace Greatbone.Core
{
    ///
    /// A JSON array model.
    ///
    public class JArr
    {
        const int InitialCapacity = 16;

        JMember[] elements;

        int count;

        internal JArr(int capacity = InitialCapacity)
        {
            elements = new JMember[capacity];
            count = 0;
        }

        public JMember this[int index] => elements[index];

        public int Count => count;

        internal void Add(JMember elem)
        {
            int len = elements.Length;
            if (count >= len)
            {
                JMember[] alloc = new JMember[len * 4];
                Array.Copy(elements, 0, alloc, 0, len);
                elements = alloc;
            }
            elements[count++] = elem;
        }

        internal void Dump<R>(ISink<R> snk) where R : ISink<R>
        {
            for (int i = 0; i < count; i++)
            {
                JMember elem = elements[i];
                JType typ = elem.type;
                if (typ == JType.Array)
                {
                    snk.Put(null, (JArr)elem);
                }
                else if (typ == JType.Object)
                {
                    snk.Put(null, (JObj)elem);
                }
                else if (typ == JType.String)
                {
                    snk.Put(null, (string)elem);
                }
                else if (typ == JType.Number)
                {
                    snk.Put(null, (JNumber)elem);
                }
                else if (typ == JType.True)
                {
                    snk.Put(null, true);
                }
                else if (typ == JType.False)
                {
                    snk.Put(null, false);
                }
                else if (typ == JType.Null)
                {
                    snk.PutNull(null);
                }
            }
        }

        public D[] ToDatas<D>(byte z = 0) where D : IData, new()
        {
            D[] dats = new D[count];
            for (int i = 0; i < dats.Length; i++)
            {
                D dat = new D();
                dat.Load((JObj)this[i], z);
                dats[i] = dat;
            }
            return dats;
        }

        public override string ToString()
        {
            JsonContent cont = new JsonContent(false, true);
            cont.Add('[');
            Dump(cont);
            cont.Add(']');
            string str = cont.ToString();
            BufferUtility.Return(cont);
            return str;
        }
    }
}