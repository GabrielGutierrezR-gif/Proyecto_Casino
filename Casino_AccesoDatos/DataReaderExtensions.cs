using System.Data.Common;
using System.Reflection;

namespace Casino_AccesoDatos
{
    // Convierte las filas que devuelve un procedimiento almacenado en una lista de objetos.
    // Empata cada columna con la propiedad que se llame igual (sin importar mayúsculas).
    public static class DataReaderExtensions
    {
        public static IList<T> MapToList<T>(this DbDataReader dr)
        {
            var objList = new List<T>();
            var props = typeof(T).GetRuntimeProperties();

            var colMapping = dr.GetColumnSchema()
                .Where(x => props.Any(y => y.Name.ToLower() == x.ColumnName.ToLower()))
                .ToDictionary(key => key.ColumnName.ToLower());

            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    T obj = Activator.CreateInstance<T>();

                    foreach (var prop in props)
                    {
                        // si la columna no viene en el resultado del SP, se ignora
                        if (colMapping.ContainsKey(prop.Name.ToLower()))
                        {
                            var val = dr.GetValue(colMapping[prop.Name.ToLower()].ColumnOrdinal!.Value);
                            prop.SetValue(obj, val == DBNull.Value ? null : val);
                        }
                    }

                    objList.Add(obj);
                }
            }

            return objList;
        }
    }
}