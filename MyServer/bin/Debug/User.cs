using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Fintreon.RolePermission.DataHelper;
using Fintreon.RolePermission.Objects;
namespace Fintreon.RolePermission.Objects
{
    public class User
    {
        public int? UserId { get; set; }
        public String UserName { get; set; }
        public String FirstName { get; set; }
        public String LastName { get; set; }
        public String Email { get; set; }
        public String PasswordHash { get; set; } //Password
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public List<UserGroup> Groups { get; set; }
        public List<Tuple<Role,Organization>> Roles { get; set; }
        public void Save()
        {
           UserDBHelper.Save(this);
        }

        public static  List<User> GetAll()
        {
            List<User> lstOfUser = DataHelper.UserDBHelper.GetAll();
            return lstOfUser;
        }
        public static  User GetById(int userId)
        {
            User user = DataHelper.UserDBHelper.GetById(userId );
            return user;
        }
        public static void AddRole(int userId, int roleId, int? organizationId)
        {
            DataHelper.UserDBHelper.AddRole(userId, roleId, organizationId);
        }
        public static void DeleteRole(int userId, int roleId, int? organizationId)
        {
            DataHelper.UserDBHelper.DeleteRole(userId, roleId, organizationId);
        }


        public static byte? CheckPermission(int userId, string permissionName)
        {
            return DataHelper.UserDBHelper.CheckPermission(userId, permissionName,null,null,null);
        }

        public static byte? CheckPermission(int userId, string permissionName, int organizationId)
        {
            return DataHelper.UserDBHelper.CheckPermission(userId, permissionName,null,null, organizationId);
        }


        public static byte? CheckPermission(int userId, byte permissionType, string externalName)
        {
            return DataHelper.UserDBHelper.CheckPermission(userId,null, permissionType, externalName, null);
        }

        public static byte? CheckPermission(int userId, byte permissionType, string externalName, int organizationId)
        {
            return DataHelper.UserDBHelper.CheckPermission(userId,null, permissionType, externalName, organizationId);
        }


        public static List<Permission> CheckPermission(int userId, List<string> permissionNames)
        {
            return DataHelper.UserDBHelper.CheckPermission(userId,null, permissionNames);
        }

        public static List<Permission> CheckPermission(int userId,List<string> permissionNames, int organizationId)
        {
            return DataHelper.UserDBHelper.CheckPermission(userId, organizationId, permissionNames);
        }
    }

}