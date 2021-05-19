
namespace TaskManagement.Common
{
    public class EStatoEsecuzione
    {
        public const short InEsecuzione = 1;
        public const short Terminato = 2;

        public const short PS_Pianificato = 1001;
        public const short PS_InEsecuzione = 1002;
        public const short PS_TerminatoConSuccesso = 1003;
        public const short PS_TerminatoConErrori = 1004;
        public const short PS_Saltato = 1005;
    }
}

