using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Mono.CSharp;

namespace SisoDb.Management
{
    public abstract class TypeMapping
    {
        protected IEnumerable<string> properties;
        protected string idKey;

        public Type Contract { get; set; }
        public Type Implementation { get; set; }
        public string ContractName { get; set; }
        public string ImplementationName { get; set; }

        public IEnumerable<string> Properties
        {
            get
            {
                if (properties == null)
                {
                    var schema = Configuration.DB.StructureSchemas.GetSchema(Contract);
                    properties = schema.IndexAccessors.Select(x => x.Path).ToList();
                }
                return properties;
            }
        }

        public string IdKey
        {
            get
            {
                if (idKey == null)
                {
                    var schema = Configuration.DB.StructureSchemas.GetSchema(Contract);
                    idKey = schema.IdAccessor.Path;
                }
                return idKey;
            }
        }

        public abstract QueryResult GetJsonList(QuerySpec query);
        public abstract int DeleteByQuery(QuerySpec query);

        public Type GetIdType()
        {
            var schema = Configuration.DB.StructureSchemas.GetSchema(Contract);
            return schema.IdAccessor.DataType;
        }

        public abstract Type GetCollectionType();
    }

    public class TypeMapping<TContract, TImpl> : TypeMapping
        where TContract : class
        where TImpl : class
    {

        public TypeMapping()
        {
            Contract = typeof(TContract);
            ContractName = Contract.Name;
            Implementation = typeof(TImpl);
            ImplementationName = Implementation.Name;
        }

        public ISisoQueryable<TContract> Query()
        {

            return Configuration.DB.UseOnceTo().Query<TContract>();
        }

        public Expression<Func<TContract, bool>> Predicate(string predicate, params string[] setup)
        {
            var evaluator = new Evaluator(new CompilerSettings(), new Report(new ConsoleReportPrinter()));
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Where(ass => ass.FullName != typeof(Evaluator).Assembly.FullName))
            {
                evaluator.ReferenceAssembly(assembly);
            }

            evaluator.Run("using System;");
            evaluator.Run("using SisoDb;");
            evaluator.Run(string.Format("using {0};", typeof(TContract).Namespace));
            evaluator.Run(string.Format("using {0};", typeof(TypeMapping).Namespace));

            foreach (var item in setup)
            {
                var splitSetup = item.Replace("\r", "").Split('\n');
                foreach (var statement in splitSetup)
                {
                    evaluator.Run(statement);
                    evaluator.Run(statement); //HACK: For some odd reason the statements only work the second time I run them
                }
            }

            var str = string.Format("QueryGenerator.Generate<{0}>({1});", Contract, predicate.Replace('"', '\u0022'));
            Expression<Func<TContract, bool>> pred;
            try
            {
                pred = evaluator.Evaluate(str) as Expression<Func<TContract, bool>>;
            }
            catch (Exception e)
            {
                try
                {
                    //HACK: For some odd reason the statements only work the second time I run them. 
                    pred = evaluator.Evaluate(str) as Expression<Func<TContract, bool>>; 
                }
                catch (Exception ex)
                {
                    throw new Exception("Mono could not parse the predicate", e);
                }
            }
            return pred;
        }

        public Expression<Func<TContract, object>> OrderBy(string orderBy)
        {
            var evaluator = new Evaluator(new CompilerSettings(), new Report(new ConsoleReportPrinter()));
            evaluator.ReferenceAssembly(typeof(TContract).Assembly);
            evaluator.ReferenceAssembly(typeof(TypeMapping).Assembly);

            evaluator.Run(string.Format("using {0};", typeof(TContract).Namespace));
            evaluator.Run(string.Format("using {0};", typeof(TypeMapping).Namespace));

            var str = string.Format("OrderByGenerator.Generate<{0}>({1});", Contract, orderBy);
            var pred = evaluator.Evaluate(str) as Expression<Func<TContract, object>>;
            return pred;
        }

        public override QueryResult GetJsonList(QuerySpec query)
        {
            var result = new QueryResult();

            var q = Query();
            if (!string.IsNullOrWhiteSpace(query.Predicate))
            {
                var pred = Predicate(query.Predicate, query.Setup.ToArray());
                q = q.Where(pred);
            }

            int? totalMatches = null;
            if (!string.IsNullOrWhiteSpace(query.OrderBy))
            {
                
                if (query.PageSize.HasValue)
                {
                    totalMatches = q.Count();
                    q = q.Page(query.Page, query.PageSize.Value);
                }

                //This is done after the paging only because the Count does not work otherwise
                var orderBy = OrderBy(query.OrderBy);
                q = query.SortOrder == SortOrder.Asc ? q.OrderBy(orderBy) : q.OrderByDescending(orderBy);
            }

            result.Entities = q.ToListOfJson();
            result.TotalMatches = totalMatches ?? result.Entities.Count();
            return result;
        }

        public override int DeleteByQuery(QuerySpec query)
        {
            var pred = Predicate(query.Predicate, query.Setup.ToArray());
            using (var session = Configuration.DB.BeginSession())
            {
                var count = Query().Count(pred);
                session.Advanced.DeleteByQuery<TContract>(pred);

                return count;
            }

        }

        public override Type GetCollectionType()
        {
            return typeof(IEnumerable<TImpl>);
        }
    }


    public static class QueryGenerator
    {
        public static Expression<Func<T, bool>> Generate<T>(Expression<Func<T, bool>> e) { return e; }
    }

    public static class OrderByGenerator
    {
        public static Expression<Func<T, object>> Generate<T>(Expression<Func<T, object>> e) { return e; }
    }

    public static class ModifierGenerator
    {
        public static Expression<Func<T, bool>> Generate<T>(Expression<Func<T, bool>> e) { return e; }
    }
}
