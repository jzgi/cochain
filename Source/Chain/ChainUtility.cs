using System;
using System.Threading.Tasks;
using SkyChain.Db;

namespace SkyChain.Chain
{
    public static class ChainUtility
    {
        public static async Task<bool> ChainStartTran(this DbContext dc, string an, short typ, string inst, string descr, decimal amt, JObj doc = null, string npeerid = null, string nan = null)
        {
            // if exists
            var def = ChainEnv.GetDefinition(typ);
            if (def == null)
            {
                throw new ChainException("definition not found: typ = " + typ);
            }
            var act = def.StartActivity;

            // padded sequence number
            var tn = (string) dc.Scalar("SELECT lpad(to_hex(nextval('chain.txn')),8, '0')");
            tn = ChainEnv.Info.id + tn;

            var op = new Operation()
            {
                tn = tn,
                step = act.step,
                an = an,
                typ = typ,
                inst = inst,
                descr = descr,
                doc = doc,
                stamp = DateTime.Now,
                npeerid = npeerid,
                nan = nan
            };

            var ok = act.OnSubmit(op, dc, false);
            if (!ok)
            {
                return false;
                // throw new ChainException("input error: tn = " + op.tn + ", step = " + op.step);
            }

            // data access
            dc.Sql("INSERT INTO ops ").colset(op)._VALUES_(op);
            await dc.ExecuteAsync(p => op.Write(p));

            return true;
        }

        internal static Block[] ChainGetBlock(this DbContext dc, short typid, int code)
        {
            dc.Query("SELECT * FROM chain.blocks WHERE typid = @1 AND key = @2", p => p.Set(typid).Set(code));
            return null;
        }

        public static byte[] ChainQuery(this DbContext dc, short typ, string an)
        {
            dc.Sql("SELECT body FROM chain.blocks WHERE typid = @1 AND key = @2");
            dc.Query(p => p.Set(typ).Set(an));
            dc.Let(out byte[] body);

            return default;
        }

        public static (int amt, int balance, DateTime stamp) ChainGet(this DbContext dc, short typ, string key, string nodeid = "&")
        {
            // var typs = Framework.Obtain<Map<short, Typ>>();

            dc.Sql("SELECT body FROM chain.blocks WHERE typid = @1 AND key = @2");
            dc.Query(p => p.Set(typ).Set(key));

            // var typ = typs[typid];
            // if (typ == null) return null;
            // if (typ.op <= 1)
            // {
            //     return null;
            // }
            //
            // var cryptokey = typ.op >= 3 ? Framework.publickey : Framework.privatekey;
            //
            // while (dc.Next())
            // {
            //     dc.Let(out byte[] body);
            //     // descrypt
            //     CryptionUtility.Decrypt(body, body.Length, cryptokey);
            //
            //     var jc = new JsonParser(body, body.Length).Parse();
            // }
            return (0, 0, DateTime.Now);
        }
    }
}