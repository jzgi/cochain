using Greatbone.Core;

namespace Greatbone.Sample
{
    ///
    /// /-shopid-/myorder/-orderid-/
    ///
    public class MyOrderVarDirectory : WebDirectory, IVar
    {
        public MyOrderVarDirectory(WebDirectoryContext dc) : base(dc)
        {
        }

        ///
        /// Get order's detail.
        ///
        public void @default(WebActionContext ac)
        {
            string shopid = ac.Var;
            int id = ac.Var2;

            using (var dc = Service.NewDbContext())
            {
                DbSql sql = new DbSql("SELECT ").columnlst(Order.Empty)._("FROM orders WHERE id = @1 AND shopid = @2");
                if (dc.Query(sql.ToString(), p => p.Put(id).Put(shopid)))
                {
                    var order = dc.ToDat<Order>();
                    ac.SendHtmlMajor(200, "", main =>
                    {

                    });
                }
                else
                    ac.SendHtmlMajor(200, "没有记录", main => { });
            }
        }

        public void cannel(WebActionContext ac)
        {
            string shopid = ac.Var;
            int orderid = ac.Var2;

            using (var dc = Service.NewDbContext())
            {
                DbSql sql = new DbSql("SELECT ").columnlst(Order.Empty)._("FROM orders WHERE id = @1 AND shopid = @2");
                if (dc.Query(sql.ToString(), p => p.Put(orderid).Put(shopid)))
                {
                    var order = dc.ToDat<Order>();
                    ac.SendHtmlMajor(200, "", main =>
                    {

                    });
                }
                else
                    ac.SendHtmlMajor(200, "没有记录", main => { });
            }
        }

        public void pend(WebActionContext ac)
        {
        }

        public void close(WebActionContext ac)
        {
        }
    }
}