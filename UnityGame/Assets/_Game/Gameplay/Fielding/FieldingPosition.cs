using System;
using UnityEngine;
using CricketGame.Fielding;

namespace CricketGame.Gameplay.Fielding
{
    public enum FieldingPosition
    {
        Bowler,
        WicketKeeper,
        Slip1,
        Slip2,
        Slip3,
        Gully,
        Point,
        Cover,
        ExtraCover,
        MidOff,
        MidOn,
        MidWicket,
        SquareLeg,
        FineLeg,
        ThirdMan,
        LongOff,
        LongOn,
        DeepCover,
        DeepMidWicket,
        DeepSquareLeg
    }

    public static class FieldingPositionUtility
    {
        public static string GetPositionName(FieldingPosition position)
        {
            return position.ToString();
        }

        public static Vector3 GetDefaultCoordinates(FieldingPosition position)
        {
            switch (position)
            {
                case FieldingPosition.Bowler: return new Vector3(0f, 0f, -10.0f);
                case FieldingPosition.WicketKeeper: return new Vector3(0f, 0f, 16.0f);
                case FieldingPosition.Slip1: return new Vector3(2.5f, 0f, 16.5f);
                case FieldingPosition.Slip2: return new Vector3(4.0f, 0f, 16.5f);
                case FieldingPosition.Slip3: return new Vector3(5.5f, 0f, 16.5f);
                case FieldingPosition.Gully: return new Vector3(9.0f, 0f, 14.0f);
                case FieldingPosition.Point: return new Vector3(18.0f, 0f, 8.0f);
                case FieldingPosition.Cover: return new Vector3(22.0f, 0f, -2.0f);
                case FieldingPosition.ExtraCover: return new Vector3(24.0f, 0f, -12.0f);
                case FieldingPosition.MidOff: return new Vector3(-8.0f, 0f, -8.0f);
                case FieldingPosition.MidOn: return new Vector3(8.0f, 0f, -8.0f);
                case FieldingPosition.MidWicket: return new Vector3(-20.0f, 0f, -2.0f);
                case FieldingPosition.SquareLeg: return new Vector3(-18.0f, 0f, 8.0f);
                case FieldingPosition.FineLeg: return new Vector3(-22.0f, 0f, 25.0f);
                case FieldingPosition.ThirdMan: return new Vector3(25.0f, 0f, 35.0f);
                case FieldingPosition.LongOff: return new Vector3(-15.0f, 0f, -45.0f);
                case FieldingPosition.LongOn: return new Vector3(15.0f, 0f, -45.0f);
                case FieldingPosition.DeepCover: return new Vector3(48.0f, 0f, -15.0f);
                case FieldingPosition.DeepMidWicket: return new Vector3(-48.0f, 0f, -15.0f);
                case FieldingPosition.DeepSquareLeg: return new Vector3(-45.0f, 0f, 20.0f);
                default: return Vector3.zero;
            }
        }

        public static Vector3 GetWorldPosition(FieldingPosition position)
        {
            if (CricketPositionManager.Instance != null)
            {
                Transform anchor = CricketPositionManager.Instance.GetFielderAnchor(GetPositionName(position));
                if (anchor != null)
                {
                    return anchor.position;
                }
            }
            return GetDefaultCoordinates(position);
        }

        public static bool IsOutfield(FieldingPosition position)
        {
            switch (position)
            {
                case FieldingPosition.ThirdMan:
                case FieldingPosition.FineLeg:
                case FieldingPosition.LongOff:
                case FieldingPosition.LongOn:
                case FieldingPosition.DeepCover:
                case FieldingPosition.DeepMidWicket:
                case FieldingPosition.DeepSquareLeg:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsCloseCatcher(FieldingPosition position)
        {
            switch (position)
            {
                case FieldingPosition.Slip1:
                case FieldingPosition.Slip2:
                case FieldingPosition.Slip3:
                case FieldingPosition.Gully:
                case FieldingPosition.WicketKeeper:
                    return true;
                default:
                    return false;
            }
        }
    }
}
