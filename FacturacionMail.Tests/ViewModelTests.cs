using FacturacionMail.ViewModels;
using FacturacionMail.Interfaces;
using FacturacionMail.Models;
using Moq;
using Xunit;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace FacturacionMail.Tests
{
    public class ViewModelTests
    {
        [Fact]
        public async Task FacturacionMailViewModel_DebeFiltrarClientesExcluidos()
        {
            var mockClienteService = new Mock<IClienteService>();
            var mockFacturaService = new Mock<IFacturaService>();
            var mockEmailService = new Mock<IEmailService>();
            var mockCapiLoggerAdapter = new Mock<IAppLogger>();

            var clientes = new List<Cliente>
            {
                new Cliente { Codigo = 11111 },
                new Cliente { Codigo = 22222 },
                new Cliente { Codigo = 33333 }
            };

            var excluidos = new List<string> { "22222" };

            mockClienteService.Setup(s => s.ObtenerClientesAsync()).ReturnsAsync(clientes);
            mockClienteService.Setup(s => s.ObtenerClientesExcluidosAsync()).ReturnsAsync(excluidos);

            var viewModel = new FacturacionMailViewModel(mockClienteService.Object, mockFacturaService.Object, mockEmailService.Object, mockCapiLoggerAdapter.Object);

            await viewModel.CargarClientesCommand.ExecuteAsync(null);

            Assert.Equal(2, viewModel.Clientes.Count);
            Assert.Contains(viewModel.Clientes, c => c.Codigo == 11111);
            Assert.Contains(viewModel.Clientes, c => c.Codigo == 33333);
            Assert.DoesNotContain(viewModel.Clientes, c => c.Codigo == 22222);
        }

        [Fact]
        public async Task EnvioFacturasPendientesViewModel_DebeFiltrarClientesExcluidos()
        {
            // Arrange
            var mockClienteService = new Mock<IClienteService>();
            var mockFacturaService = new Mock<IFacturaService>();
            var mockEmailService = new Mock<IEmailService>();
            var mockCapiLoggerAdapter = new Mock<IAppLogger>();

            var clientes = new List<Cliente>
            {
                new Cliente { Codigo = 100 },
                new Cliente { Codigo = 200 }
            };

            var excluidos = new List<string> { "200" };

            mockClienteService.Setup(s => s.ObtenerClientesAsync()).ReturnsAsync(clientes);
            mockClienteService.Setup(s => s.ObtenerClientesExcluidosAsync()).ReturnsAsync(excluidos);

            var viewModel = new EnvioFacturasPendientesViewModel(mockClienteService.Object, mockFacturaService.Object, mockEmailService.Object, mockCapiLoggerAdapter.Object);

            var method = viewModel.GetType().GetMethod("CargarClientesAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (method != null)
            {
                var task = (Task?)method.Invoke(viewModel, null);
                if (task != null) await task;
            }

            // Assert
            Assert.Single(viewModel.Clientes);
            Assert.Equal(100, viewModel.Clientes[0].Codigo);
        }
    }
}
