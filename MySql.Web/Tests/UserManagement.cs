// Copyright (C) 2007 MySQL AB
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License version 2 as published by
// the Free Software Foundation
//
// There are special exceptions to the terms and conditions of the GPL 
// as it is applied to this software. View the full text of the 
// exception in file EXCEPTIONS in the directory of this software 
// distribution.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

//  This code was contributed by Sean Wright (srwright@alcor.concordia.ca) on 2007-01-12
//  The copyright was assigned and transferred under the terms of
//  the MySQL Contributor License Agreement (CLA)

using NUnit.Framework;
using System.Web.Security;
using System.Collections.Specialized;
using System.Data;
using System;
using System.Configuration.Provider;
using MySql.Web.Security;

namespace MySql.Web.Tests
{
    [TestFixture]
    public class UserManagement : BaseWebTest
    {
        private MySQLMembershipProvider provider;

        [SetUp]
		public override void Setup()
		{
			base.Setup();
			execSQL("DROP TABLE IF EXISTS mysql_membership");
		}

        private void CreateUserWithFormat(MembershipPasswordFormat format)
        {
            provider = new MySQLMembershipProvider();
            NameValueCollection config = new NameValueCollection();
            config.Add("connectionStringName", "LocalMySqlServer");
            config.Add("applicationName", "/");
            config.Add("passwordStrengthRegularExpression", "bar.*");
            config.Add("passwordFormat", format.ToString());
            provider.Initialize(null, config);

            // create the user
            MembershipCreateStatus status;
            provider.CreateUser("foo", "barbar!", "foo@bar.com", null, null, true, null, out status);
            Assert.AreEqual(MembershipCreateStatus.Success, status);

            // verify that the password format is hashed.
            DataTable table = FillTable("SELECT * FROM my_aspnet_Membership");
            MembershipPasswordFormat rowFormat =
                (MembershipPasswordFormat)Convert.ToInt32(table.Rows[0]["PasswordFormat"]);
            Assert.AreEqual(format, rowFormat);

            //  then attempt to verify the user
            Assert.IsTrue(provider.ValidateUser("foo", "barbar!"));
        }

        [Test]
        public void CreateUserWithHashedPassword()
        {
            CreateUserWithFormat(MembershipPasswordFormat.Hashed);
        }

        [Test]
        public void CreateUserWithEncryptedPasswordWithAutoGenKeys()
        {
            try
            {
                CreateUserWithFormat(MembershipPasswordFormat.Encrypted);
            }
            catch (ProviderException)
            {
            }
        }

        [Test]
        public void CreateUserWithClearPassword()
        {
            CreateUserWithFormat(MembershipPasswordFormat.Clear);
        }

        /// <summary>
        /// Bug #34792 New User/Changing Password Validation Not working. 
        /// </summary>
        [Test]
        public void ChangePassword()
        {
            CreateUserWithHashedPassword();
            try
            {
                provider.ChangePassword("foo", "barbar!", "bar2");
                Assert.Fail();
            }
            catch (ArgumentException ae1)
            {
                Assert.AreEqual("newPassword", ae1.ParamName);
                Assert.IsTrue(ae1.Message.Contains("length of parameter"));
            }

            try
            {
                provider.ChangePassword("foo", "barbar!", "barbar2");
                Assert.Fail();
            }
            catch (ArgumentException ae1)
            {
                Assert.AreEqual("newPassword", ae1.ParamName);
                Assert.IsTrue(ae1.Message.Contains("alpha numeric"));
            }

            // now test regex strength testing
            bool result = provider.ChangePassword("foo", "barbar!", "zzzxxx!");
            Assert.IsFalse(result);

            // now do one that should work
            result = provider.ChangePassword("foo", "barbar!", "barfoo!");
            Assert.IsTrue(result);

            provider.ValidateUser("foo", "barfoo!");
        }

        /// <summary>
        /// Bug #34792 New User/Changing Password Validation Not working. 
        /// </summary>
        [Test]
        public void CreateUserWithErrors()
        {
            provider = new MySQLMembershipProvider();
            NameValueCollection config = new NameValueCollection();
            config.Add("connectionStringName", "LocalMySqlServer");
            config.Add("applicationName", "/");
            config.Add("passwordStrengthRegularExpression", "bar.*");
            config.Add("passwordFormat", "Hashed");
            provider.Initialize(null, config);

            // first try to create a user with a password not long enough
            MembershipCreateStatus status;
            MembershipUser user = provider.CreateUser("foo", "xyz", 
                "foo@bar.com", null, null, true, null, out status);
            Assert.IsNull(user);
            Assert.AreEqual(MembershipCreateStatus.InvalidPassword, status);

            // now with not enough non-alphas
            user = provider.CreateUser("foo", "xyz1234",
                "foo@bar.com", null, null, true, null, out status);
            Assert.IsNull(user);
            Assert.AreEqual(MembershipCreateStatus.InvalidPassword, status);

            // now one that doesn't pass the regex test
            user = provider.CreateUser("foo", "xyzxyz!",
                "foo@bar.com", null, null, true, null, out status);
            Assert.IsNull(user);
            Assert.AreEqual(MembershipCreateStatus.InvalidPassword, status);

            // now one that works
            user = provider.CreateUser("foo", "barbar!",
                "foo@bar.com", null, null, true, null, out status);
            Assert.IsNotNull(user);
            Assert.AreEqual(MembershipCreateStatus.Success, status);
        }

        [Test]
        public void DeleteUser()
        {
            CreateUserWithHashedPassword();
            Assert.IsTrue(provider.DeleteUser("foo", true));
            DataTable table = FillTable("SELECT * FROM my_aspnet_Membership");
            Assert.AreEqual(0, table.Rows.Count);
            table = FillTable("SELECT * FROM my_aspnet_Users");
            Assert.AreEqual(0, table.Rows.Count);

            CreateUserWithHashedPassword();
            provider = new MySQLMembershipProvider();
            NameValueCollection config = new NameValueCollection();
            config.Add("connectionStringName", "LocalMySqlServer");
            config.Add("applicationName", "/");
            provider.Initialize(null, config);
            Assert.IsTrue(Membership.DeleteUser("foo", false));
            table = FillTable("SELECT * FROM my_aspnet_Membership");
            Assert.AreEqual(0, table.Rows.Count);
            table = FillTable("SELECT * FROM my_aspnet_Users");
            Assert.AreEqual(1, table.Rows.Count);
        }

        [Test]
        public void FindUsersByName()
        {
            CreateUserWithHashedPassword();

            int records;
            MembershipUserCollection users = provider.FindUsersByName("F%", 0, 10, out records);
            Assert.AreEqual(1, records);
            Assert.AreEqual("foo", users["foo"].UserName);
        }

        [Test]
        public void FindUsersByEmail()
        {
            CreateUserWithHashedPassword();

            int records;
            MembershipUserCollection users = provider.FindUsersByEmail("foo@bar.com", 0, 10, out records);
            Assert.AreEqual(1, records);
            Assert.AreEqual("foo", users["foo"].UserName);
        }

        [Test]
        public void TestCreateUserOverrides()
        {
            try
            {
				// we have to initialize the provider so the db will exist
/*				provider = new MySQLMembershipProvider();
				NameValueCollection config = new NameValueCollection();
				config.Add("connectionStringName", "LocalMySqlServer");
				config.Add("applicationName", "/");
				provider.Initialize(null, config);
*/				
				Membership.CreateUser("foo", "bar");
                int records;
                MembershipUserCollection users = Membership.FindUsersByName("F%", 0, 10, out records);
                Assert.AreEqual(1, records);
                Assert.AreEqual("foo", users["foo"].UserName);

                Membership.CreateUser("test", "bar", "myemail@host.com");
                users = Membership.FindUsersByName("T%", 0, 10, out records);
                Assert.AreEqual(1, records);
                Assert.AreEqual("test", users["test"].UserName);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [Test]
        public void NumberOfUsersOnline()
        {
            int numOnline = Membership.GetNumberOfUsersOnline();
            Assert.AreEqual(0, numOnline);

            Membership.CreateUser("foo", "bar");
            Membership.CreateUser("foo2", "bar");

            numOnline = Membership.GetNumberOfUsersOnline();
            Assert.AreEqual(2, numOnline);
        }

        [Test]
        public void UnlockUser()
        {
            Membership.CreateUser("foo", "bar");
            Assert.IsFalse(Membership.ValidateUser("foo", "bar2"));
            Assert.IsFalse(Membership.ValidateUser("foo", "bar3"));
            Assert.IsFalse(Membership.ValidateUser("foo", "bar3"));
            Assert.IsFalse(Membership.ValidateUser("foo", "bar3"));
            Assert.IsFalse(Membership.ValidateUser("foo", "bar3"));

            // the user should be locked now so the right password should fail
            Assert.IsFalse(Membership.ValidateUser("foo", "bar"));

            MembershipUser user = Membership.GetUser("foo");
            Assert.IsTrue(user.IsLockedOut);

            Assert.IsTrue(user.UnlockUser());
            user = Membership.GetUser("foo");
            Assert.IsFalse(user.IsLockedOut);

            Assert.IsTrue(Membership.ValidateUser("foo", "bar"));
        }

        [Test]
        public void GetUsernameByEmail()
        {
            Membership.CreateUser("foo", "bar", "foo@bar.com");
            string username = Membership.GetUserNameByEmail("foo@bar.com");
            Assert.AreEqual("foo", username);

            username = Membership.GetUserNameByEmail("foo@b.com");
            Assert.IsNull(username);

            username = Membership.GetUserNameByEmail("  foo@bar.com   ");
            Assert.AreEqual("foo", username);
        }

        [Test]
        public void UpdateUser()
        {
            MembershipCreateStatus status;
            Membership.CreateUser("foo", "bar", "foo@bar.com", "color", "blue", true, out status);
            Assert.AreEqual(MembershipCreateStatus.Success, status);

            MembershipUser user = Membership.GetUser("foo");

            user.Comment = "my comment";
            user.Email = "my email";
            user.IsApproved = false;
            user.LastActivityDate = new DateTime(2008, 1, 1);
            user.LastLoginDate = new DateTime(2008, 2, 1);
            Membership.UpdateUser(user);

            MembershipUser newUser = Membership.GetUser("foo");
            Assert.AreEqual(user.Comment, newUser.Comment);
            Assert.AreEqual(user.Email, newUser.Email);
            Assert.AreEqual(user.IsApproved, newUser.IsApproved);
            Assert.AreEqual(user.LastActivityDate, newUser.LastActivityDate);
            Assert.AreEqual(user.LastLoginDate, newUser.LastLoginDate);
        }

        private void ChangePasswordQAHelper(MembershipUser user, string pw, string newQ, string newA)
        {
            try
            {
                user.ChangePasswordQuestionAndAnswer(pw, newQ, newA);
                Assert.Fail("This should not work.");
            }
            catch (ArgumentNullException ane)
            {
                Assert.AreEqual("password", ane.ParamName);
            }
            catch (ArgumentException)
            {
                Assert.IsNotNull(pw);
            }
        }

        [Test]
        public void ChangePasswordQuestionAndAnswer()
        {
            MembershipCreateStatus status;
            Membership.CreateUser("foo", "bar", "foo@bar.com", "color", "blue", true, out status);
            Assert.AreEqual(MembershipCreateStatus.Success, status);

            MembershipUser user = Membership.GetUser("foo");
            ChangePasswordQAHelper(user, "", "newQ", "newA");
            ChangePasswordQAHelper(user, "bar", "", "newA");
            ChangePasswordQAHelper(user, "bar", "newQ", "");
            ChangePasswordQAHelper(user, null, "newQ", "newA");

            bool result = user.ChangePasswordQuestionAndAnswer("bar", "newQ", "newA");
            Assert.IsTrue(result);

            user = Membership.GetUser("foo");
            Assert.AreEqual("newQ", user.PasswordQuestion);
        }

        [Test]
        public void GetAllUsers()
        {
            // first create a bunch of users
            for (int i=0; i < 100; i++)
                Membership.CreateUser(String.Format("foo{0}", i), "bar");

            MembershipUserCollection users = Membership.GetAllUsers();
            Assert.AreEqual(100, users.Count);
            int index = 0;
            foreach (MembershipUser user in users)
                Assert.AreEqual(String.Format("foo{0}", index++), user.UserName);

            int total;
            users = Membership.GetAllUsers(2, 10, out total);
            Assert.AreEqual(10, users.Count);
            Assert.AreEqual(100, total);
            index = 0;
            foreach (MembershipUser user in users)
                Assert.AreEqual(String.Format("foo2{0}", index++), user.UserName);
        }

        private void GetPasswordHelper(bool requireQA, bool enablePasswordRetrieval, string answer)
        {
            MembershipCreateStatus status;
            provider = new MySQLMembershipProvider();
            NameValueCollection config = new NameValueCollection();
            config.Add("connectionStringName", "LocalMySqlServer");
            config.Add("requiresQuestionAndAnswer", requireQA ? "true" : "false");
            config.Add("enablePasswordRetrieval", enablePasswordRetrieval ? "true" : "false");
            config.Add("passwordFormat", "clear");
            config.Add("applicationName", "/");
            provider.Initialize(null, config);

            provider.CreateUser("foo", "bar", "foo@bar.com", "color", "blue", true, null, out status);

            try
            {
                string password = provider.GetPassword("foo", answer);
                if (!enablePasswordRetrieval)
                    Assert.Fail("This should have thrown an exception");
                Assert.AreEqual("bar", password);
            }
            catch (ProviderException)
            {
                if (requireQA && answer != null)
                    Assert.Fail("This should not have thrown an exception");
            }
        }

        [Test]
        public void GetPassword()
        {
            GetPasswordHelper(false, false, null);
            GetPasswordHelper(false, true, null);
            GetPasswordHelper(true, true, null);
            GetPasswordHelper(true, true, "blue");
        }

        [Test]
        public void GetUser()
        {
            Membership.CreateUser("foo", "bar");
            MembershipUser user = Membership.GetUser(1);
            Assert.AreEqual("foo", user.UserName);

            // now move the activity date back outside the login
            // window
            user.LastActivityDate = new DateTime(2008, 1, 1);
            Membership.UpdateUser(user);

            user = Membership.GetUser("foo");
            Assert.IsFalse(user.IsOnline);

            user = Membership.GetUser("foo", true);
            Assert.IsTrue(user.IsOnline);

            // now move the activity date back outside the login
            // window again
            user.LastActivityDate = new DateTime(2008, 1, 1);
            Membership.UpdateUser(user);

            user = Membership.GetUser(1);
            Assert.IsFalse(user.IsOnline);

            user = Membership.GetUser(1, true);
            Assert.IsTrue(user.IsOnline);
        }

        [Test]
        public void FindUsers()
        {
            for (int i=0; i < 100; i++)
                Membership.CreateUser(String.Format("boo{0}", i), "bar");
            for (int i=0; i < 100; i++)
                Membership.CreateUser(String.Format("foo{0}", i), "bar");
            for (int i=0; i < 100; i++)
                Membership.CreateUser(String.Format("schmoo{0}", i), "bar");


            MembershipUserCollection users = Membership.FindUsersByName("fo%");
            Assert.AreEqual(100, users.Count);

            int total;
            users = Membership.FindUsersByName("fo%", 2, 10, out total);
            Assert.AreEqual(10, users.Count);
            Assert.AreEqual(100, total);
            int index = 0;
            foreach (MembershipUser user in users)
                Assert.AreEqual(String.Format("foo2{0}", index++), user.UserName);

        }
    }
}
