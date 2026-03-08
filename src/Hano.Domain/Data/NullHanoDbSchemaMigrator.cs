using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Hano.Data;

/* This is used if database provider does't define
 * IHanoDbSchemaMigrator implementation.
 */
public class NullHanoDbSchemaMigrator : IHanoDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
