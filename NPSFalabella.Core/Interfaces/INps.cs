using NPSFalabella.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NPSFalabella.Core.Interfaces
{
    public interface INps
    {
        Task<IReadOnlyList<Registro>> ListarTramas();
        Task<bool> UpdateTrama(string ContactId, int Procesado, string Resultado);
        Task<bool> ProcesarTrama();

    }
}
