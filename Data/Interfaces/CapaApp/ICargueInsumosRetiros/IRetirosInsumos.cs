using System.Security.Cryptography;

namespace Data.Interfaces.CapaApp.ICargueInsumosRetiros;

public interface IRetirosInsumos
{

    //metodos para insertar la carga en bd
    Task ImportarVinculadoDesdeExcel(Stream fileStream, string fileName);
    Task ImportarDADesdeExcel(Stream fileStream, string fileName);
    Task ImportarTerceroDesdeExcel(Stream fileStream, string fileName);

}
