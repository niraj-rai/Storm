using System.Collections.Generic;
using System.Data;

namespace Storm.Core
{
    public interface ISqlQuery
    {
        List<IDbDataParameter> Parameters { get; }
        string ToString();
    }
}
