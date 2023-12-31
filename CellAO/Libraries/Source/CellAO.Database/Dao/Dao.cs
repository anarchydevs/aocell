﻿#region License

// Copyright (c) 2005-2014, CellAO Team
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

#endregion

namespace CellAO.Database.Dao
{
    #region Usings ...

    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Reflection;

    using CellAO.Core.Exceptions;
    using CellAO.Database.Entities;

    using Dapper;

    #endregion

    /// <summary>
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    /// <typeparam name="TU">
    /// </typeparam>
    public class Dao<T, TU> : IDao<T>
        where T : IDBEntity, new()
        where TU : class, IDao<T>
    {
        /// <summary>
        /// </summary>
        public static TU Instance
        {
            get
            {
                return SingletonCreator.instance;
            }
        }

        #region Static Fields

        /// <summary>
        /// </summary>
        private static Dictionary<string, PropertyInfo> cachedProperties = null;

        #endregion

        #region Public Properties

        /// <summary>
        /// </summary>
        /// <summary>
        /// </summary>
        public string TableName
        {
            get
            {
                return getTablename();
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Cache the properties for fast access
        /// </summary>
        protected static Dictionary<string, PropertyInfo> CachedProperties
        {
            get
            {
                if (cachedProperties == null)
                {
                    cachedProperties = new Dictionary<string, PropertyInfo>();
                    lock (cachedProperties)
                    {
                        cachedProperties.Clear();
                        foreach (PropertyInfo property in
                            typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
                        {
                            cachedProperties.Add(property.Name, property);
                        }
                    }
                }

                return cachedProperties;
            }
        }

        /// <summary>
        /// </summary>
        /// <returns>
        /// </returns>
        /// <exception cref="Exception">
        /// </exception>
        protected static string getTablename()
        {
            if (!typeof(T).GetCustomAttributes(typeof(TablenameAttribute), false).Any())
            {
                throw new Exception("You forgot to set the TablenameAttribute on class " + typeof(T).FullName);
            }

            return
                ((TablenameAttribute)typeof(T).GetCustomAttributes(typeof(TablenameAttribute), false).First()).Tablename;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// </summary>
        /// <param name="entity">
        /// </param>
        /// <param name="connection">
        /// </param>
        /// <param name="transaction">
        /// </param>
        /// <returns>
        /// </returns>
        /// <exception cref="DataBaseException">
        /// </exception>
        public int Add(
            T entity,
            IDbConnection connection = null,
            IDbTransaction transaction = null,
            bool dontUseId = true)
        {
            int rowsAffected = 0;

            IDbConnection conn = connection;
            try
            {
                conn = conn ?? Connector.GetConnection();
                IDbTransaction trans = transaction;
                try
                {
                    trans = trans ?? conn.BeginTransaction();
                    rowsAffected = conn.Execute(
                        SqlMapperUtil.CreateInsertSQL(this.TableName, entity, dontUseId),
                        entity,
                        trans);

                    // Does this need to be inside of the transaction or outside?? -- Algorithman

                    // we must retrive the Id anyway here. we need to standardise the id as we started to do.
                    if (rowsAffected == 1)
                    {
                        SqlMapperUtil.SetIdentity<int>(conn, id => entity.Id = id == 0 ? entity.Id : id, trans);
                    }
                    else
                    {
                        throw new DataBaseException(
                            string.Format("Failed to create new record on table '{0}'", this.TableName));
                    }
                }
                finally
                {
                    if (transaction == null)
                    {
                        if (trans != null)
                        {
                            trans.Commit();
                            trans.Dispose();
                        }
                    }
                }
            }
            finally
            {
                if (connection == null)
                {
                    if (conn != null)
                    {
                        conn.Dispose();
                    }
                }
            }

            return rowsAffected;
        }

        /// <summary>
        /// Delete a row from the table
        /// </summary>
        /// <param name="entityId">
        /// id of the row to delete
        /// </param>
        /// <param name="connection">
        /// </param>
        /// <param name="transaction">
        /// optional transaction for the delete
        /// </param>
        /// <returns>
        /// </returns>
        public int Delete(int entityId, IDbConnection connection = null, IDbTransaction transaction = null)
        {
            int rowsAffected = 0;
            IDbConnection conn = connection;
            try
            {
                conn = conn ?? Connector.GetConnection();
                IDbTransaction trans = transaction;
                try
                {
                    trans = trans ?? conn.BeginTransaction();
                    rowsAffected = conn.Execute(
                        SqlMapperUtil.CreateDeleteSQL(this.TableName),
                        new { id = entityId },
                        transaction);
                }
                finally
                {
                    if (transaction == null)
                    {
                        if (trans != null)
                        {
                            trans.Commit();
                            trans.Dispose();
                        }
                    }
                }
            }
            finally
            {
                if (connection == null)
                {
                    if (conn != null)
                    {
                        conn.Dispose();
                    }
                }
            }

            return rowsAffected;
        }

        /// <summary>
        /// Delete rows from table
        /// </summary>
        /// <param name="whereParameters">
        /// optional DynamicParameters with the items for the where-clause
        /// </param>
        /// <param name="connection">
        /// </param>
        /// <param name="transaction">
        /// optional transaction for the delete
        /// </param>
        /// <returns>
        /// </returns>
        public int Delete(object whereParameters, IDbConnection connection = null, IDbTransaction transaction = null)
        {
            int rowsAffected = 0;
            IDbConnection conn = connection;
            try
            {
                conn = conn ?? Connector.GetConnection();
                IDbTransaction trans = transaction;
                try
                {
                    trans = trans ?? conn.BeginTransaction();
                    rowsAffected = conn.Execute(
                        SqlMapperUtil.CreateDeleteSQL(this.TableName, whereParameters),
                        whereParameters,
                        transaction);
                }
                finally
                {
                    if (transaction == null)
                    {
                        if (trans != null)
                        {
                            trans.Commit();
                            trans.Dispose();
                        }
                    }
                }
            }
            finally
            {
                if (connection == null)
                {
                    if (conn != null)
                    {
                        conn.Dispose();
                    }
                }
            }

            return rowsAffected;
        }

        /// <summary>
        /// </summary>
        /// <param name="entityId">
        /// </param>
        /// <returns>
        /// </returns>
        public bool Exists(int entityId)
        {
            bool exists = false;
            using (IDbConnection conn = Connector.GetConnection())
            {
                exists =
                    conn.Query<int>(
                        string.Format("SELECT ID FROM {0} where ID = @id", this.TableName),
                        new { id = entityId }).Count() == 1;
            }

            return exists;
        }

        /// <summary>
        /// Load a Character by id
        /// </summary>
        /// <param name="entityId">
        /// Id of the Character
        /// </param>
        /// <returns>
        /// DBCharacter object
        /// </returns>
        public T Get(int entityId)
        {
            T entity = default(T);
            using (IDbConnection conn = Connector.GetConnection())
            {
                entity =
                    conn.Query<T>(
                        SqlMapperUtil.CreateGetSQL(this.TableName, new { Id = entityId }),
                        new { Id = entityId }).SingleOrDefault();
            }

            return entity;
        }

        /// <summary>
        /// Load all Character data
        /// </summary>
        /// <param name="parameters">
        /// </param>
        /// <returns>
        /// Collection of DBCharacter
        /// </returns>
        public IEnumerable<T> GetAll(object parameters = null)
        {
            IEnumerable<T> entities = null;
            using (IDbConnection conn = Connector.GetConnection())
            {
                entities = conn.Query<T>(SqlMapperUtil.CreateGetSQL(this.TableName, parameters), parameters);
            }

            return entities;
        }

        /// <summary>
        /// </summary>
        /// <param name="entity">
        /// </param>
        /// <param name="parameters">
        /// </param>
        /// <param name="connection">
        /// </param>
        /// <param name="transaction">
        /// </param>
        /// <returns>
        /// </returns>
        public int Save(
            T entity,
            object parameters = null,
            IDbConnection connection = null,
            IDbTransaction transaction = null)
        {
            int rowsAffected = 0;

            IDbConnection conn = connection;
            try
            {
                conn = conn ?? Connector.GetConnection();
                IDbTransaction trans = transaction;
                try
                {
                    trans = trans ?? conn.BeginTransaction();
                    rowsAffected = conn.Execute(
                        SqlMapperUtil.CreateUpdateSQL(this.TableName, parameters ?? entity),
                        parameters ?? entity,
                        transaction);
                }
                finally
                {
                    if (transaction == null)
                    {
                        if (trans != null)
                        {
                            trans.Commit();
                            trans.Dispose();
                        }
                    }
                }
            }
            finally
            {
                if (connection == null)
                {
                    if (conn != null)
                    {
                        conn.Dispose();
                    }
                }
            }

            return rowsAffected;
        }

        /// <summary>
        /// </summary>
        /// <param name="entities">
        /// </param>
        /// <param name="parameters">
        /// </param>
        /// <param name="connection">
        /// </param>
        /// <param name="transaction">
        /// </param>
        /// <returns>
        /// </returns>
        public int Save(
            List<T> entities,
            object parameters = null,
            IDbConnection connection = null,
            IDbTransaction transaction = null)
        {
            int rowsAffected = 0;

            IDbConnection conn = connection;
            try
            {
                conn = conn ?? Connector.GetConnection();
                IDbTransaction trans = transaction;
                try
                {
                    trans = trans ?? conn.BeginTransaction();
                    foreach (T entity in entities)
                    {
                        rowsAffected += Save(entity, null, conn, trans); // Pass parameters instead of null here? 
                    }
                }
                finally
                {
                    if (transaction == null)
                    {
                        if (trans != null)
                        {
                            trans.Commit();
                            trans.Dispose();
                        }
                    }
                }
            }
            finally
            {
                if (connection == null)
                {
                    if (conn != null)
                    {
                        conn.Dispose();
                    }
                }
            }

            return rowsAffected;
        }

        #endregion

        #region Methods

        /// <summary>
        /// </summary>
        /// <param name="item">
        /// </param>
        /// <returns>
        /// </returns>
        private DynamicParameters getAllParameters(T item)
        {
            DynamicParameters parameters = new DynamicParameters();
            foreach (string propertyName in CachedProperties.Keys)
            {
                parameters.Add(propertyName, CachedProperties[propertyName].GetValue(item, null));
            }

            return parameters;
        }

        public IEnumerable<T> GetWhere(
            object parameter = null,
            IDbConnection connection = null,
            IDbTransaction transaction = null)
        {
            IEnumerable<T> result = new List<T>();

            IDbConnection conn = connection;
            try
            {
                conn = conn ?? Connector.GetConnection();
                IDbTransaction trans = transaction;
                try
                {
                    result = conn.Query<T>(SqlMapperUtil.CreateGetSQL(this.TableName, parameter), parameter, trans);
                }
                finally
                {
                    if (transaction == null)
                    {
                        if (trans != null)
                        {
                            trans.Commit();
                            trans.Dispose();
                        }
                    }
                }
            }
            finally
            {
                if (connection == null)
                {
                    if (conn != null)
                    {
                        conn.Dispose();
                    }
                }
            }
            return result;
        }

        #endregion

        /// <summary>
        /// </summary>
        private class SingletonCreator
        {
            /// <summary>
            /// </summary>
            internal static readonly TU instance = Activator.CreateInstance<TU>();

            /// <summary>
            /// </summary>
            static SingletonCreator()
            {
            }
        }
    }
}