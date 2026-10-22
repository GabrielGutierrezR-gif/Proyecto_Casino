using Casino_AccesoDatos.Contexto;
using Casino_Dominio.Entidades;
using Casino_Dominio.InterfacesAD;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Casino_AccesoDatos.Implementaciones
{
    public class UnidadTrabajoEF : IUnidadTrabajoEF
    {
        private readonly CasinoContext _contexto;
        private IDbContextTransaction? _transaccion;

        // Usuarios y compras
        public IRepositorioAD<Usuario> TUsuario { get; }
        public IRepositorioAD<MetodosPago> TMetodosPago { get; }
        public IRepositorioAD<PaquetesFicha> TPaquetesFichas { get; }
        public IRepositorioAD<Compra> TCompra { get; }

        // Blackjack
        public IRepositorioAD<SalasBlackjack> TSalasBlackjack { get; }
        public IRepositorioAD<JugadoresBlackjack> TJugadoresBlackjack { get; }
        public IRepositorioAD<RondasBlackjack> TRondasBlackjack { get; }
        public IRepositorioAD<ApuestasBlackjack> TApuestasBlackjack { get; }

        // Ruleta
        public IRepositorioAD<PartidasRuletum> TPartidasRuleta { get; }
        public IRepositorioAD<ApuestasRuletum> TApuestasRuleta { get; }

        // Administración
        public IRepositorioAD<Configuracion> TConfiguracion { get; }

        public UnidadTrabajoEF(CasinoContext contexto)
        {
            _contexto = contexto;

            TUsuario = new RepositorioAD<Usuario>(_contexto);
            TMetodosPago = new RepositorioAD<MetodosPago>(_contexto);
            TPaquetesFichas = new RepositorioAD<PaquetesFicha>(_contexto);
            TCompra = new RepositorioAD<Compra>(_contexto);

            TSalasBlackjack = new RepositorioAD<SalasBlackjack>(_contexto);
            TJugadoresBlackjack = new RepositorioAD<JugadoresBlackjack>(_contexto);
            TRondasBlackjack = new RepositorioAD<RondasBlackjack>(_contexto);
            TApuestasBlackjack = new RepositorioAD<ApuestasBlackjack>(_contexto);

            TPartidasRuleta = new RepositorioAD<PartidasRuletum>(_contexto);
            TApuestasRuleta = new RepositorioAD<ApuestasRuletum>(_contexto);

            TConfiguracion = new RepositorioAD<Configuracion>(_contexto);
        }

        public int Completar()
        {
            return _contexto.SaveChanges();
        }

        public void EmpezarTransaccion()
        {
            _transaccion = _contexto.Database.BeginTransaction();
        }

        public void CompletarTran()
        {
            if (_transaccion != null)
            {
                _transaccion.Commit();
                _transaccion.Dispose();
                _transaccion = null;
            }
        }

        public void Rollback()
        {
            if (_transaccion != null)
            {
                _transaccion.Rollback();
                _transaccion.Dispose();
                _transaccion = null;
            }
        }

        public void CerrarConexion()
        {
            _contexto.Database.CloseConnection();
        }

        public void Dispose()
        {
            _transaccion?.Dispose();
            _contexto.Dispose();
        }
    }
}