using System;
using System.Collections.Generic;
using AwiUtils;

namespace ChessSharp.Pieces
{
    /// <summary>Represents the promotion of the pawn.</summary>
    public enum PawnPromotion
    {
        /// <summary>Promote the pawn to a knight.</summary>
        Knight,
        /// <summary>Promote the pawn to a bishop.</summary>
        Bishop,
        /// <summary>Promote the pawn to a rook.</summary>
        Rook,
        /// <summary>Promote the pawn to a queen.</summary>
        Queen,
    }

    public static class PawnPromotionHelper
    {
        static PawnPromotionHelper()
        {
            charToPPDict = new Dictionary<string, PawnPromotion>(StringComparer.InvariantCultureIgnoreCase);
            var dict = Helper.ToDictionary("N Knight B Bishop R Rook Q Queen");
            foreach (var kvp in dict)
                charToPPDict.Add(kvp.Key, (PawnPromotion)Enum.Parse(typeof(PawnPromotion), kvp.Value));
        }

        static public PawnPromotion Get(char c) => charToPPDict["" + c];
        static Dictionary<string, PawnPromotion> charToPPDict;
    }

    public class PawnPromotionEx
    {
        public PawnPromotionEx(PawnPromotion to, string name) 
        {
            To = to;
            Name = name;
        }
        public PawnPromotion To { get; set; }
        public string Name { get; set; }
        public override string ToString() => Name;
    }

}