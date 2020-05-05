using NBsoft.Wordzz.Contracts;
using NBsoft.Wordzz.Contracts.Entities;
using NBsoft.Wordzz.Core.Repositories;
using NBsoft.Wordzz.Entities;
using System.Threading.Tasks;

namespace NBsoft.Wordzz.Extensions
{
    internal static class ContractExtensions
    {  
        public static T ToDto<T>(this object src) where T : new()
        {
            var res = new T();
            foreach (var prop in src.GetType().GetProperties())
            {
                var value = prop.GetValue(src);
                prop.SetValue(res, value, null);
                
            }

            return res;
        }


        public static GameDataModel ToDataModel(this IGame src)
        {
            return new GameDataModel
            {
                Id = src.Id,
                BoardId = src.Board.Id,
                Language = src.Language,
                CreationDate = src.CreationDate,
                Player01 = src.Player01.UserName,
                Player01Rack = src.Player01.Rack.GetString(),
                Player02 = src.Player02.UserName,
                Player02Rack = src.Player02.Rack.GetString(),
                Status = (int)src.Status,
                CurrentPlayer = src.CurrentPlayer,
                CurrentStart = src.CurrentStart,
                CurrentPauseStart = src.CurrentPauseStart,
                LetterBag = src.LetterBag.Bag.ToJson(),
                Winner = src.Winner,
                FinishReason = (int?)src.FinishReason,
                ConsecutivePasses = src.ConsecutivePasses,
                FinishDate = src.FinishDate,
                P1FinalScore = src.P1FinalScore,
                P2FinalScore = src.P2FinalScore
            };
        }
        public static GameMoveDataModel ToDataModel(this IPlayMove src)
        {
            return new GameMoveDataModel
            {
                PlayerId =  src.Player,
                PlayStart = src.PlayStart,
                PlayFinish = src.PlayFinish,
                Score = src.Score,
                Letters = src.Letters.ToJson(),
                Words = src.Words.ToJson()
            };
        }


    }
}
