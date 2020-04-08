using System;
using System.Data.Common;
using System.Net.Security;
using CloudUn.Db;

namespace CloudUn.Net
{
    public static class ChainDbUtility
    {
        public static R[] ChainQuery<R>(this DbContext dc, short typ, int code) where R : IData, new()
        {
            dc.Query("SELECT * FROM un.blocks WHERE typ = @1 AND keyno = @2", p => p.Set(typ).Set(code));
            return null;
        }

        internal static Block[] ChainGetBlock(this DbContext dc, short typ, int code)
        {
            dc.Query("SELECT * FROM un.blocks WHERE typ = @1 AND keyno = @2", p => p.Set(typ).Set(code));
            return null;
        }

        public static byte[] ChainQuery<M>(this DbContext dc, short datypid, string key) where M : IData
        {
            dc.Sql("SELECT body FROM chain.blocks WHERE datypid = @1 AND key = @2");
            dc.Query(p => p.Set(datypid).Set(key));
            dc.Let(out byte[] body);


            return default;
        }

        public static M[] ChainGet<M>(this DbContext dc, short datypid, string key) where M : IData
        {
            dc.Sql("SELECT body FROM chain.blocks WHERE datypid = @1 AND key = @2");
            dc.Query(p => p.Set(datypid).Set(key));

            var datypes = Framework.Obtain<Map<short, DataTyp>>();
            var dattyp = datypes[datypid];
            if (dattyp.op <= 1)
            {
                return null;
            }

            var cryptokey = dattyp.op >= 3 ? Framework.publickey : Framework.privatekey;

            while (dc.Next())
            {
                dc.Let(out byte[] body);
                // descrypt
                CryptionUtility.Decrypt(body, body.Length, cryptokey);
                
                var jc = new JsonParser(body, body.Length).Parse();
            }
            return null;
        }

        public static void ChainPut(this DbContext dc, short datypid, string key, string[] tags, DynamicContent content)
        {
            // retrieve prior hash

            // calculate new hash based on prior hash and the content
            var datypes = Framework.Obtain<Map<short, DataTyp>>();
            var dattyp = datypes[datypid];
            if (dattyp.op <= 1)
            {
                return;
            }

            var cryptokey = dattyp.op >= 3 ? Framework.publickey : Framework.privatekey;
            CryptionUtility.Encrypt(content.Buffer, content.Count, cryptokey); // encrypt
            var prior = (string) dc.Scalar("SELECT hash FROM chain.blocks WHERE datypid = @1 ORDER BY seq DESC LIMIT 1", p => p.Set(datypid));
            string hash = content.MD5(prior);
            var body = new ArraySegment<byte>(content.Buffer, 0, content.Count);

            // record insertion
            dc.Sql("INSERT INTO chain.blocks (datypid, key, tags, body, hash) VALUES (@1, @2, @3, @4, @5)");
            dc.Execute(p => p.Set(datypid).Set(key).Set(tags).Set(body).Set(hash));
        }
    }
}