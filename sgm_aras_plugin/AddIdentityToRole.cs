﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Aras.IOM;
using Aras.Server.Core;
using System.Windows.Forms;

namespace sgm_aras_plugin
{
   public class AddIdentityToRole
    {
    public static void sgmAddIdentityToRoleUsePos(Item user,string role,CallContext CCO)
        {
           string identityId= CCO.Identity.GetIdentityIdByUserId(user.getID());
            Innovator inn = user.getInnovator();
           Item Identity = inn.getItemById("Identity",identityId);
            string aml = "<AML>";
            aml += "<Item type='List' action='get'>";
            aml += "    <name>{0}</name>";
            aml += "	<Relationships>";
            aml += "		<Item type='Value' select='label,value'>";
            aml += "		</Item>";
            aml += "	</Relationships>";
            aml += "</Item>";
            aml += "</AML>";
            aml = string.Format(aml, role);
            Item list = inn.applyAML(aml);

            XmlNodeList ndsList = list.node.SelectNodes("Relationships/Item[@type='Value']/value");

            List<string> posList = new List<string>();

            for(int i = 0; i < ndsList.Count; i++)
            {
               string pos_desc=ndsList[i].InnerText;
                posList.Add(pos_desc);

            }
            string user_pos_desc = user.getProperty("position_desc");
            int index=posList.FindIndex(s => s == user_pos_desc);


            Item Role = inn.getItemById("Identity",CCO.Identity.GetIdentityIdByName(role));


            if (index>=0)
            {
                if (!isMember(Role, Identity, inn))
                {
                    Item member = inn.newItem("Member", "add");
                    member.setProperty("source_id", Role.getID());
                    member.setProperty("related_id", identityId );
                    member = member.apply();
                }
            }
            else
            {
                if (isMember(Role, Identity, inn))
                {
                    string sql = "DELETE FROM Member WHERE source_id ='{0}' and related_id='{1}'";
                    sql = string.Format(sql, Role.getID(), identityId);
                    Item temp= inn.applySQL(sql);
                }
            }

        }
    public static void sgmAddIdentityToRoleUseDept(Item user, string role, CallContext CCO)
        {
            string identityId = CCO.Identity.GetIdentityIdByUserId(user.getID());
            Innovator inn = user.getInnovator();
            Item Identity = inn.getItemById("Identity", identityId);
            string aml = "<AML>";
            aml += "<Item type='List' action='get'>";
            aml += "    <name>{0}</name>";
            aml += "	<Relationships>";
            aml += "		<Item type='Value' select='lable,value'>";
            aml += "		</Item>";
            aml += "	</Relationships>";
            aml += "</Item>";
            aml += "</AML>";
            aml = string.Format(aml, role);
            Item list = inn.applyAML(aml);

            XmlNodeList ndsList = list.node.SelectNodes("Relationships/Item[@type='Value']/value");

            List<string> deptList = new List<string>();

            for (int i = 0; i < ndsList.Count; i++)
            {
                string dept = ndsList[i].InnerText;
                deptList.Add(dept);

            }
            string user_dept = user.getProperty("department");
            int index = deptList.FindIndex(s => s == user_dept);


            Item Role = inn.getItemById("Identity", CCO.Identity.GetIdentityIdByName(role));


            if (index >= 0)
            {
                if (!isMember(Role, Identity, inn))
                {
                    Item member = inn.newItem("Member", "add");
                    member.setProperty("source_id", Role.getID());
                    member.setProperty("related_id", identityId);
                    member = member.apply();
                }
            }
            else
            {
                if (isMember(Role, Identity, inn))
                {
                    string sql = "DELETE FROM Member WHERE source_id ='{0}' and related_id='{1}'";
                    sql = string.Format(sql, Role.getID(), identityId);
                    Item temp = inn.applySQL(sql);
                }
            }

        }
    public static bool isMember(Item identity1, Item identity2, Innovator inn)
        {
            string id1 = identity1.getID();
            string id2 = identity2.getID();
            Item member = inn.newItem("Member", "get");
            member.setProperty("source_id", id1);
            member.setProperty("related_id", id2);
            member = member.apply();
            if (member.getItemCount() > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void setAliasName(Item user, CallContext CCO)
        {
            string aliasId = CCO.Identity.GetUserAliases(user.getID());
            Innovator inn = user.getInnovator();
            Item userAlias = inn.getItemById("Identity", aliasId);
            string aml = "<AML>";
            aml += "<Item type='ItemType' action='get'>";
            aml += "    <name>User</name>";
            aml += "	<Relationships>";
            aml += "		<Item type='Property' select='name,keyed_name_order' orderBy='keyed_name_order'>";
            aml += "            <keyed_name_order condition = 'is not null' />";
            aml += "		</Item>";
            aml += "	</Relationships>";
            aml += "</Item>";
            aml += "</AML>";
        
            Item userType = inn.applyAML(aml);
            XmlNodeList ndsList = userType.node.SelectNodes("Relationships/Item[@type='Property']/name");

            string tagName = "";
            for (int i = 0; i < ndsList.Count; i++)
            {
                string propName = ndsList[i].InnerText;
                tagName = tagName+" "+user.getProperty(propName);
            }
            tagName = tagName.Trim();
            if (tagName!="")
            {

                string sql = "UPDATE [innovator].[IDENTITY] set name='{0}' WHERE id='{1}'";
                sql = string.Format(sql, tagName, aliasId);
                Item temp = inn.applySQL(sql);
            }

        }

    }

}
