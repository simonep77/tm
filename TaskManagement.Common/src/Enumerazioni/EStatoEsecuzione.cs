
namespace TaskManagement.Common
{
    public enum EStatoEsecuzione
    {
        InEsecuzione = 1,
        Terminato = 2,

        PS_Pianificato = 1001,
        PS_InEsecuzione = 1002,
        PS_TerminatoConSuccesso = 1003,
        PS_TerminatoConErrori = 1004,
        PS_Saltato = 1005
    }
}

