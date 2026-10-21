using AutoMapper;
using Casino_Dominio.Entidades;
using Casino_Dominio.EntidadesTipadas;

namespace Casino_Dominio.DTO
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // Usuarios y compras
            CreateMap<Usuario, TUsuario>().ReverseMap();
            CreateMap<MetodosPago, TMetodosPago>().ReverseMap();
            CreateMap<PaquetesFicha, TPaquetesFichas>().ReverseMap();
            CreateMap<Compra, TCompra>().ReverseMap();

            // Blackjack
            CreateMap<SalasBlackjack, TSalasBlackjack>().ReverseMap();
            CreateMap<JugadoresBlackjack, TJugadoresBlackjack>().ReverseMap();
            CreateMap<RondasBlackjack, TRondasBlackjack>().ReverseMap();
            CreateMap<ApuestasBlackjack, TApuestasBlackjack>().ReverseMap();

            // Ruleta
            CreateMap<PartidasRuletum, TPartidasRuleta>().ReverseMap();
            CreateMap<ApuestasRuletum, TApuestasRuleta>().ReverseMap();

            // Administración
            CreateMap<Configuracion, TConfiguracion>().ReverseMap();
        }
    }
}