using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Andy.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace AndyTest
{
    [TestClass]
    public class PropertyAccessorTest
    {
        private class Cust { public string ID { get; set; } public string Name { get; set; } }
        private class Supp { public string ID { get; set; } public string Name { get; set; } }
        private class Emp { public string EmployeeID { get; set; } public string EmployeeName { get; set; } }

        [TestMethod]
        public void TestClone()
        {
            var customer001 = new Cust { ID = "C001", Name = "A客戶" };
            var customer002 = PropertyAccessor.Clone(customer001);
            Assert.AreNotSame(customer001, customer002);
            Assert.AreEqual(customer001.ID, customer002.ID );
            Assert.AreEqual(customer001.Name, customer002.Name );
        }

        [TestMethod]
        public void TestIsValueEqual()
        {
            var customer001 = new Cust { ID = "C001", Name = "A客戶" };
            var customer002 = new Cust { ID = "C001", Name = "A客戶" };
            Assert.IsTrue(PropertyAccessor.IsValueEqual(customer001, customer002));
        }

        [TestMethod]
        public void TestCloneMany()
        {
            var customerlist1 = new List<Cust>();
            customerlist1.Add(new Cust { ID = "C001", Name = "A客戶" });
            customerlist1.Add(new Cust { ID = "C002", Name = "B客戶" });
            customerlist1.Add(new Cust { ID = "C003", Name = "C客戶" });
            var customerlist2 = PropertyAccessor.CloneMany(customerlist1).ToList();
            Assert.AreEqual(customerlist1.Count, customerlist2.Count);
        }

        [TestMethod]
        public void TestManyValueEqual()
        {
            var customerlist1 = new List<Cust>();
            customerlist1.Add(new Cust { ID = "C001", Name = "A客戶" });
            customerlist1.Add(new Cust { ID = "C002", Name = "B客戶" });
            customerlist1.Add(new Cust { ID = "C003", Name = "C客戶" });
            var customerlist2 = new List<Cust>();
            customerlist2.Add(new Cust { ID = "C001", Name = "A客戶" });
            customerlist2.Add(new Cust { ID = "C002", Name = "B客戶" });
            customerlist2.Add(new Cust { ID = "C003", Name = "C客戶" });
            var isEqual = PropertyAccessor.IsManyValueEqual(customerlist1, customerlist2);
            Assert.IsTrue(isEqual);
        }

        [TestMethod]
        public void TestAutoMap()
        {
            var customer = new Cust { ID = "C001", Name = "A客戶" };
            var supplier = new Supp { ID = "S001", Name = "A廠商" };
            PropertyAccessor.AutoMapTo(customer, supplier);
            Assert.AreEqual(customer.ID, supplier.ID);
        }

        [TestMethod]
        public void TestMapTo()
        {
            var customer = new Cust { ID = "C001", Name = "A客戶" };
            var customerfields = new string[] { "ID", "Name" };
            var employee = new Emp();
            var employfields = new string[] { "EmployeeID", "EmployeeName" };
            PropertyAccessor.MapTo(customer, customerfields, employee, employfields);
            Assert.AreEqual(customer.ID, employee.EmployeeID);
        }

        [TestMethod]
        public void TestIndexerSetter()
        {
            var customer = new Cust();
            var customerPa = PropertyAccessor.Create(customer);
            customerPa["ID"] = "C001";
            customerPa["Name"] = "A客戶";
            Assert.AreEqual("C001", customer.ID);
            Assert.AreEqual("A客戶", customer.Name);
        }       
        
        [TestMethod]
        public void TestIndexerGetter()
        {
            var customer = new Cust { ID = "C001", Name = "A客戶" };
            var customerPa = PropertyAccessor.Create(customer);
            Assert.AreEqual("C001", customerPa["ID"]);
            Assert.AreEqual("A客戶", customerPa["Name"]);
        }

        [TestMethod]
        public void TestGet()
        {
            var customer = new Cust { ID = "C001", Name = "A客戶" };
            string id = PropertyAccessor.Create(customer).Get<string>("Id");
            Assert.AreEqual("C001", id);
        }
    }
}
