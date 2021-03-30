using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Threading.Tasks;

namespace RemoteControl.BLL.AD
{
    public static class ActiveDirectoryHelper
    {
        const string userType = "i:0#.w|";

        const string adDomain = "";
        const string adUserName = "";
        const string adPassword = "";

        /// <summary>
        /// Gets the base principal context
        /// </summary>
        /// <returns>Retruns the PrincipalContext object</returns>
        private static PrincipalContext GetPrincipalContext()
        {
            PrincipalContext principalContext;

            principalContext = new PrincipalContext(ContextType.Domain, adDomain, null, ContextOptions.Negotiate, adDomain + @"\" + adUserName, adPassword);

            return principalContext;
        }

        public static List<UserPrincipal> GetAllUsers()
        {
            try
            {
                var list = new List<UserPrincipal>();
                var principalContext = GetPrincipalContext();

                using (var searcher = new PrincipalSearcher(new UserPrincipal(principalContext)))
                {
                    foreach (var result in searcher.FindAll())
                    {
                        DirectoryEntry de = result.GetUnderlyingObject() as DirectoryEntry;
                        // Если имени входа или фамилии нет, пропускаем
                        if (de.Properties["userPrincipalName"].Value == null || de.Properties["sn"].Value == null || de.Properties["givenName"].Value == null)
                            continue;
                        var userPrincipal = UserPrincipal.FindByIdentity(principalContext, (string)de.Properties["userPrincipalName"].Value);
                        if (userPrincipal != null)
                            list.Add(userPrincipal);
                        Console.WriteLine("First Name: " + de.Properties["givenName"].Value);
                        Console.WriteLine("Last Name : " + de.Properties["sn"].Value);
                        Console.WriteLine("SAM account name   : " + de.Properties["samAccountName"].Value);
                        Console.WriteLine("User principal name: " + de.Properties["userPrincipalName"].Value);
                        Console.WriteLine();
                    }
                }

                return list.OrderBy(u => u.Name).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Gets the AD Attributes not represented in UserPrincipal
        /// </summary>
        /// <param name="principal">The User Principal Object</param>
        /// <param name="property">The property name you want to retrieve</param>
        /// <returns></returns>
        public static string GetProperty(this UserPrincipal principal, string property)
        {
            var directoryEntry = principal.GetUnderlyingObject() as DirectoryEntry;

            if (directoryEntry.Properties.Contains(property))
            {
                return directoryEntry.Properties[property].Value.ToString();
            }
            else
            {
                //Property not exisiting return empty strung
                return string.Empty;
            }
        }
    }
}
