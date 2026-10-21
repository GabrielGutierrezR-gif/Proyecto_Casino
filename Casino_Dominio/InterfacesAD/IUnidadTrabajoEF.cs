using System;
using System.Collections.Generic;
using System.Text;
using Casino_Dominio.Entidades;

namespace Casino_Dominio.InterfacesAD
{
    public interface IUnidadTrabajoEF : IDisposable
    {
       
        IRepositorioAD<Usuario> TUsuario { get; }
        IRepositorioAD<MetodosPago> TMetodosPago { get; }
        IRepositorioAD<PaquetesFicha> TPaquetesFichas { get; }
        IRepositorioAD<Compra> TCompra { get; }

        IRepositorioAD<SalasBlackjack> TSalasBlackjack { get; }
        IRepositorioAD<JugadoresBlackjack> TJugadoresBlackjack { get; }
        IRepositorioAD<RondasBlackjack> TRondasBlackjack { get; }
        IRepositorioAD<ApuestasBlackjack> TApuestasBlackjack { get; }

      
        IRepositorioAD<PartidasRuletum> TPartidasRuleta { get; }
        IRepositorioAD<ApuestasRuletum> TApuestasRuleta { get; }

        IRepositorioAD<Configuracion> TConfiguracion { get; }

     
        int Completar();
        void EmpezarTransaccion();
        void CompletarTran();
        void Rollback();
        void CerrarConexion();
    }
}