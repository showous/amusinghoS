﻿using amusinghoS.EntityData;
using amusinghoS.EntityData.Base;
using amusinghoS.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace amusinghoS.Services.Base
{
    public class RepositoryBase<T> where T : amusingBase
    {
        private readonly DbSet<T> _dbSet;
        public amusinghoSDbContext DbContext { get; } = null;
        public RepositoryBase(amusinghoSDbContext context)
        {
            DbContext = context;
            _dbSet = DbContext.Set<T>();
        }
        public DatabaseFacade Database => DbContext.Database;
        public IQueryable<T> Entities => _dbSet.AsQueryable().AsNoTracking();
        public int SaveChanges()
        {
            return DbContext.SaveChanges();
        }
        public async Task<int> SaveChangesAsync()
        {
            return await DbContext.SaveChangesAsync();
        }
        public bool Any(Expression<Func<T, bool>> whereLambd)
        {
            return _dbSet.Where(whereLambd).Any();
        }
        public Task<bool> AnyAsync(Expression<Func<T, bool>> whereLambd)
        {
            return _dbSet.Where(whereLambd).AnyAsync();
        }
        public void Disposed()
        {
            throw new Exception("不允许在这里释放上下文，请在UnitOfWork中操作");
            //DbContext.Dispose();
        }

        #region 插入数据
        public bool Insert(T entity, bool isSaveChange = true)
        {
            _dbSet.Add(entity);
            if (isSaveChange)
            {
                return SaveChanges() > 0;
            }
            return false;
        }
        public async Task<bool> InsertAsync(T entity, bool isSaveChange = true)
        {
            _dbSet.Add(entity);
            if (isSaveChange)
            {
                return await SaveChangesAsync() > 0;
            }
            return false;
        }
        public bool Insert(List<T> entitys, bool isSaveChange = true)
        {
            _dbSet.AddRange(entitys);
            if (isSaveChange)
            {
                return SaveChanges() > 0;
            }
            return false;
        }
        public async Task<bool> InsertAsync(List<T> entitys, bool isSaveChange = true)
        {
            _dbSet.AddRange(entitys);
            if (isSaveChange)
            {
                return await SaveChangesAsync() > 0;
            }
            return false;
        }
        #endregion

        #region 删除
        public bool Delete(T entity, bool isSaveChange = true)
        {
            _dbSet.Attach(entity);
            _dbSet.Remove(entity);
            return isSaveChange ? SaveChanges() > 0 : false;
        }
        public bool Delete(List<T> entitys, bool isSaveChange = true)
        {
            entitys.ForEach(entity =>
            {
                _dbSet.Attach(entity);
                _dbSet.Remove(entity);
            });
            return isSaveChange ? SaveChanges() > 0 : false;
        }

        public virtual async Task<bool> DeleteAsync(T entity, bool isSaveChange = true)
        {

            _dbSet.Attach(entity);
            _dbSet.Remove(entity);
            return isSaveChange ? await SaveChangesAsync() > 0 : false;
        }
        public virtual async Task<bool> DeleteAsync(List<T> entitys, bool isSaveChange = true)
        {
            entitys.ForEach(entity =>
            {
                _dbSet.Attach(entity);
                _dbSet.Remove(entity);
            });
            return isSaveChange ? await SaveChangesAsync() > 0 : false;
        }
        #endregion

        #region 更新数据
        public bool Update(T entity, bool isSaveChange = true, List<string> updatePropertyList = null, bool modified = true)
        {
            if (entity == null)
            {
                return false;
            }
            _dbSet.Attach(entity);
            var entry = DbContext.Entry(entity);
            if (updatePropertyList == null)
            {
                entry.State = EntityState.Modified;//全字段更新
            }
            else
            {
                if (modified)
                {
                    updatePropertyList.ForEach(c => {
                        entry.Property(c).IsModified = true; //部分字段更新的写法
                    });
                }
                else
                {
                    entry.State = EntityState.Modified;//全字段更新
                    updatePropertyList.ForEach(c => {
                        entry.Property(c).IsModified = false; //部分字段不更新的写法,没测试可能有BUG
                    });
                }
            }
            if (isSaveChange)
            {
                return SaveChanges() > 0;
            }
            return false;
        }
        public bool Update(List<T> entitys, bool isSaveChange = true)
        {
            if (entitys == null || entitys.Count == 0)
            {
                return false;
            }
            entitys.ForEach(c => {
                Update(c, false);
            });
            if (isSaveChange)
            {
                return SaveChanges() > 0;
            }
            return false;
        }
        public async Task<bool> UpdateAsync(T entity, bool isSaveChange = true, List<string> updatePropertyList = null, bool modified = true)
        {
            if (entity == null)
            {
                return false;
            }
            _dbSet.Attach(entity);
            var entry = DbContext.Entry<T>(entity);
            if (updatePropertyList == null)
            {
                entry.State = EntityState.Modified;//全字段更新
            }
            else
            {
                if (modified)
                {
                    updatePropertyList.ForEach(c => {
                        entry.Property(c).IsModified = true; //部分字段更新的写法
                    });
                }
                else
                {
                    entry.State = EntityState.Modified;//全字段更新
                    updatePropertyList.ForEach(c => {
                        entry.Property(c).IsModified = false; //部分字段不更新的写法
                    });
                }
            }
            if (isSaveChange)
            {
                return await SaveChangesAsync() > 0;
            }
            return false;
        }
        public async Task<bool> UpdateAsync(List<T> entitys, bool isSaveChange = true)
        {
            if (entitys == null || entitys.Count == 0)
            {
                return false;
            }
            entitys.ForEach(c => {
                _dbSet.Attach(c);
                DbContext.Entry<T>(c).State = EntityState.Modified;
            });
            if (isSaveChange)
            {
                return await SaveChangesAsync() > 0;
            }
            return false;
        }
        #endregion

        #region 查找
        public long Count(Expression<Func<T, bool>> predicate = null)
        {
            if (predicate == null)
            {
                predicate = c => true;
            }
            return _dbSet.LongCount(predicate);
        }
        public async Task<long> CountAsync(Expression<Func<T, bool>> predicate = null)
        {
            if (predicate == null)
            {
                predicate = c => true;
            }
            return await _dbSet.LongCountAsync(predicate);
        }
        public T Get(object id)
        {
            if (id == null)
            {
                return default(T);
            }
            return _dbSet.Find(id);
        }
        public T Get(Expression<Func<T, bool>> predicate = null, bool isNoTracking = true)
        {
            var data = isNoTracking ? _dbSet.Where(predicate).AsNoTracking() : _dbSet.Where(predicate);
            if (data.ToList().Count == 0)
            {
                return null;
            }
            return data.FirstOrDefault();
        }
        public async Task<T> GetAsync(object id)
        {
            if (id == null)
            {
                return default(T);
            }
            return await _dbSet.FindAsync(id);
        }
        public async Task<T> GetAsync(Expression<Func<T, bool>> predicate = null, bool isNoTracking = true)
        {
            if (predicate == null)
                predicate = c => true;
            var data = isNoTracking ? _dbSet.Where(predicate).AsNoTracking() : _dbSet.Where(predicate);
            return await data.FirstOrDefaultAsync();
        }
        public async Task<List<T>> GetListAsync(Expression<Func<T, bool>> predicate = null, string ordering = "", bool isNoTracking = true)
        {
            if (predicate == null)
                predicate = c => true;
            var data = isNoTracking ? _dbSet.Where(predicate).AsNoTracking() : _dbSet.Where(predicate);
            if (!string.IsNullOrEmpty(ordering))
            {
                data = data.OrderByBatch(ordering);
            }
            return await data.ToListAsync();
        }
        public List<T> GetList(Expression<Func<T, bool>> predicate = null, string ordering = "", bool isNoTracking = true)
        {
            if (predicate == null)
                predicate = c => true;
            var data = isNoTracking ? _dbSet.Where(predicate).AsNoTracking() : _dbSet.Where(predicate);
            if (!string.IsNullOrEmpty(ordering))
            {
                data = data.OrderByBatch(ordering);
            }
            return data.ToList();
        }
        public List<T> GetAll(Expression<Func<T, bool>> predicate = null, bool isNoTracking = true)
        {
            if (predicate == null)
                predicate = c => true;
            var data = isNoTracking ? _dbSet.Where(predicate).AsNoTracking() : _dbSet.Where(predicate);
            return data.ToList();
        }
        public async Task<IQueryable<T>> LoadAsync(Expression<Func<T, bool>> predicate = null, bool isNoTracking = true)
        {
            if (predicate == null)
                predicate = c => true;
            return await Task.Run(() => isNoTracking ? _dbSet.Where(predicate).AsNoTracking() : _dbSet.Where(predicate));
        }
        public IQueryable<T> Load(Expression<Func<T, bool>> predicate = null, bool isNoTracking = true)
        {
            if (predicate == null)
                predicate = c => true;
            return isNoTracking ? _dbSet.Where(predicate).AsNoTracking() : _dbSet.Where(predicate);
        }
        #region 分页查找
        /// <summary>
        /// 分页查询异步
        /// </summary>
        /// <param name="whereLambda">查询添加（可有，可无）</param>
        /// <param name="ordering">排序条件（一定要有）</param>
        /// <param name="pageIndex">当前页码</param>
        /// <param name="pageSize">每页大小</param>
        /// <param name="isOrder">排序正反</param>
        /// <returns></returns>
        public async Task<PageData<T>> GetPageAsync<TKey>(int pageIndex, int pageSize,Expression<Func<T, bool>> whereLambda = null, bool isOrder = true, bool isNoTracking = true)
        {
            IQueryable<T> data = _dbSet;
            if (whereLambda != null)
            {
                data = isNoTracking ? data.Where(whereLambda).AsNoTracking() : data.Where(whereLambda);
            }
            PageData<T> pageData = new PageData<T>
            {
                Totals = await data.CountAsync(),
                Rows = await data.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync()
            };
            return pageData;
        }

        /// <summary>
        /// 分页查询异步
        /// </summary>
        /// <param name="whereLambda">查询添加（可有，可无）</param>
        /// <param name="ordering">排序条件（一定要有，多个用逗号隔开，倒序开头用-号）</param>
        /// <param name="pageIndex">当前页码</param>
        /// <param name="pageSize">每页大小</param>
        /// <returns></returns>
        public async Task<PageData<T>> GetPageAsync(Expression<Func<T, bool>> whereLambda, string ordering, int pageIndex, int pageSize, bool isNoTracking = true)
        {
            // 分页 一定注意： Skip 之前一定要 OrderBy
            if (string.IsNullOrEmpty(ordering))
            {
                ordering = nameof(T) + "Id";//默认以Id排序
            }
            var data = _dbSet.OrderByBatch(ordering);
            if (whereLambda != null)
            {
                data = isNoTracking ? data.Where(whereLambda).AsNoTracking() : data.Where(whereLambda);
            }
            //查看生成的sql，找到大数据下分页巨慢原因为order by 耗时
            //var sql = data.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToSql();
            //File.WriteAllText(@"D:\sql.txt",sql);
            PageData<T> pageData = new PageData<T>
            {
                Totals = await data.CountAsync(),
                Rows = await data.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync()
            };
            return pageData;
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="whereLambda">查询添加（可有，可无）</param>
        /// <param name="ordering">排序条件（一定要有，多个用逗号隔开，倒序开头用-号）</param>
        /// <param name="pageIndex">当前页码</param>
        /// <param name="pageSize">每页大小</param>
        /// <returns></returns>
        public PageData<T> GetPage(Expression<Func<T, bool>> whereLambda, string ordering, int pageIndex, int pageSize, bool isNoTracking = true)
        {
            // 分页 一定注意： Skip 之前一定要 OrderBy
            if (string.IsNullOrEmpty(ordering))
            {
                ordering = nameof(T) + "Id";//默认以Id排序
            }
            var data = _dbSet.OrderByBatch(ordering);
            if (whereLambda != null)
            {
                data = isNoTracking ? data.Where(whereLambda).AsNoTracking() : data.Where(whereLambda);
            }
            PageData<T> pageData = new PageData<T>
            {
                Totals = data.Count(),
                Rows = data.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList()
            };
            return pageData;
        }
        #endregion
        #endregion
    }
}
