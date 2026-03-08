using System.Threading.Tasks;

namespace Hano.Data;

public interface IHanoDbSchemaMigrator
{
    Task MigrateAsync();
}
