using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyConsume.Domain.DTO
{
    public class OddsModel
    {
        public string Bookmaker { get; set; }
        public Int64? BookmakerId { get; set; }
        public bool? BookmakerParticipantsReordered { get; set; }
        public string Competition { get; set; }
        public Int64? CompetitionId { get; set; }
        public string Country { get; set; }
        public Int64? CountryId { get; set; }
        public string FixtureGroup { get; set; }
        public Int64? FixtureGroupId { get; set; }
        public Int64? FixtureId { get; set; }
        public string GameState { get; set; }
        public bool? InRunning { get; set; }
        public bool? IsTeam { get; set; }
        public Int64? LegacyCompetitionId { get; set; }
        public Int64? LegacyCountryId { get; set; }
        public Int64? LegacyFixtureGroupId { get; set; }
        public Int64? LegacyFixtureId { get; set; }
        public Int64? LegacyParticipant1Id { get; set; }
        public Int64? LegacyParticipant2Id { get; set; }
        public Int64? LegacySportId { get; set; }
        public Int64? LineId { get; set; }
        public string MarketParameters { get; set; }
        public string MarketPeriod { get; set; }
        public string MessageId { get; set; }
        public string OptaId { get; set; }
        public string Participant1 { get; set; }
        public Int64? Participant1Id { get; set; }
        public bool? Participant1IsHome { get; set; }
        public string Participant2 { get; set; }
        public Int64? Participant2Id { get; set; }
        public IList<string> PriceNames { get; set; }
        public IList<decimal> Prices { get; set; }
        public string Sport { get; set; }
        public Int64? SportId { get; set; }
        public string StartTime { get; set; }
        public string SuperOddsType { get; set; }
        public Int64? Ts { get; set; }
        public Int64? TsOD { get; set; }
        public Int64? TsODCP { get; set; }
        public Int64? TsOPIn { get; set; }
    }
}
