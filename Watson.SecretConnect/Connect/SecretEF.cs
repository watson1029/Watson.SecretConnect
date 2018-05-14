using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Watson.SecretConnect
{
    public class SecretEF : DbContext
    {
        public SecretEF(string nameOrConnectionString)
            : base(ConnectDecrypt.GetConnectionString(nameOrConnectionString)) { }
        public SecretEF(string nameOrConnectionString, DbCompiledModel model)
            : base(ConnectDecrypt.GetConnectionString(nameOrConnectionString), model) { }
        public SecretEF(DbConnection existingConnection, bool contextOwnsConnection)
            : base(ConnectDecrypt.GetConnectionString(existingConnection.ConnectionString)) { }
        public SecretEF(ObjectContext objectContext, bool dbContextOwnsObjectContext)
            : base(objectContext, dbContextOwnsObjectContext) { }
        public SecretEF(DbConnection existingConnection, DbCompiledModel model, bool contextOwnsConnection)
            : base(ConnectDecrypt.GetConnectionString(existingConnection.ConnectionString), model) { }
        protected SecretEF()
            : base() { }
        protected SecretEF(DbCompiledModel model)
            : base() { }
    }
}
