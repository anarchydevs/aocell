﻿#region License

// Copyright (c) 2005-2014, CellAO Team
// 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
// 
//     * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//     * Neither the name of the CellAO Team nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.
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

#endregion

namespace CellAO.Database.Dao
{
    #region Usings ...

    using System;
    using System.Data;
    using System.Linq;

    using Dapper;

    using Utility;
    using System.Collections.Generic;

    #endregion

    /// <summary>
    /// Data access object for Organization queries
    /// </summary>
    public static class MobTemplateDao
    {
        // note in the following SQL MobMeshs,AdditionalMeshs are not included (big objects)
        const string SQL = "SELECT Hash,MinLvl,MaxLvl,Side,Fatness,Breed,Sex,Race,Name,Flags,NPCFamily,Health,MonsterData,MonsterScale,TextureHands,TextureBody,TextureFeet,TextureArms,TextureLegs,HeadMesh,DropHashes,DropSlots,DropRates FROM mobTemplate ";
      
        /// <summary>
        /// Retrieves a mob template by its hash code 
        /// </summary>
        public static DBMobTemplate GetMobTemplateByHash(string hash)
        {
            try
            {                
                using (IDbConnection conn = Connector.GetConnection())
                {
                    return
                        conn.Query<DBMobTemplate>(SQL + "WHERE Hash like @hash", new { hash })
                            .SingleOrDefault();
                }
            }
            catch (Exception e)
            {
                LogUtil.Debug("Searched for mobTemplate with hash:" + hash);
                LogUtil.ErrorException(e);
                throw;
            }
        }

        /// <summary>
        /// List mob templates containing by name
        /// </summary>
        public static IEnumerable<DBMobTemplate> GetMobTemplatesByName(string name, bool strictFind)
        {
            try
            {
                using (IDbConnection conn = Connector.GetConnection())
                {
                    return conn.Query<DBMobTemplate>(
                        string.Concat( SQL , "WHERE Name like " ,
                        strictFind ? string.Empty : "'%' + ",
                        " @name ", 
                        strictFind ? string.Empty : "'%' ",
                        " ORDER BY Name ASC"),
                        new { name }
                    );
                }
            }
            catch (Exception e)
            {
                LogUtil.Debug("Searched for mobTemplate with name containing :" + name);
                LogUtil.ErrorException(e);
                throw;
            }
        }

      
    }
}