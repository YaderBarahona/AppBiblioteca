using System.Threading.Tasks;
using AppBiblioteca.Models;

namespace AppBiblioteca.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(EmailMessage message);

        // Métodos específicos para notificaciones del sistema
        Task<bool> SendReservaConfirmationAsync(string userEmail, string userName, int idPrestamo, string[] materiales);
        Task<bool> SendReservaAprobadaAsync(string userEmail, string userName, int idPrestamo, string[] materiales, DateTime fechaDevolucion);
        Task<bool> SendRecordatorioVencimientoAsync(string userEmail, string userName, int idPrestamo, string[] materiales, DateTime fechaDevolucion);
        Task<bool> SendNotificacionAtrasoAsync(string userEmail, string userName, int idPrestamo, string[] materiales, int diasAtraso);
        Task<bool> SendDevolucionConfirmadaAsync(string userEmail, string userName, int idPrestamo, string[] materiales);
        Task<bool> SendReservaCanceladaAsync(string userEmail, string userName, int idPrestamo, string motivo);
    }
}