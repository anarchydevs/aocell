﻿#region License

// Copyright (c) 2005-2013, CellAO Team
// 
// 
// All rights reserved.
// 
// 
// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
// 
// 
//     * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//     * Neither the name of the CellAO Team nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.
// 
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
// CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
// EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
// PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
// PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
// LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
// 
// Last modified: 2013-10-29 22:26
// Created:       2013-10-29 20:29

#endregion

namespace CellAO.Database.Dao
{
    #region Usings ...

    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;

    using Dapper;

    #endregion

    public static class LoginDataDao
    {
        public static IEnumerable<DBLoginData> GetAll()
        {
            using (IDbConnection conn = Connector.GetConnection())
            {
                return conn.Query<DBLoginData>("SELECT * FROM login");
            }
        }

        public static DBLoginData GetByUsername(string username)
        {
            using (IDbConnection conn = Connector.GetConnection())
            {
                try
                {
                    return
                        conn.Query<DBLoginData>("SELECT * FROM login where Username=@user", new { user = username })
                            .First();
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public static void WriteLoginData(DBLoginData login)
        {
            using (IDbConnection conn = Connector.GetConnection())
            {
                conn.Execute(
                    "INSERT INTO login (CreationDate, Email, FirstName, LastName, Username, Password, Allowed_Characters, Flags, AccountFlags, Expansions, GM) VALUES (@creationdate, @email, @firstname, @lastname,@username, @password, @allowed_characters, @flags, @accountflags, @expansions, @gm)",
                    new
                        {
                            creationdate = DateTime.Now,
                            email = login.Email,
                            firstname = login.FirstName,
                            lastname = login.LastName,
                            username = login.Username,
                            password = login.Password,
                            allowed_characters = login.Allowed_Characters,
                            flags = login.Flags,
                            accountflags = login.AccountFlags,
                            expansions = login.Expansions,
                            gm = login.GM
                        });
            }
        }

        public static void WriteNewPassword(DBLoginData login)
        {
            using (IDbConnection conn = Connector.GetConnection())
            {
                conn.Execute(
                    "UPDATE login SET password=@pwd WHERE Username=@user",
                    new { pwd = login.Password, user = login.Username });
            }
        }
    }
}