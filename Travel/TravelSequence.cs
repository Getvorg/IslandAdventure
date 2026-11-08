using System;

namespace IslandAdventure.Travel
{
    public enum TravelPhase { Idle, Departing, Loading, Arriving }
    public enum TravelAction { None, LoadDestination, Finish }

    // Timing is independent of rendering and world loading. Loading has no timer.
    public sealed class TravelSequence
    {
        public const float DepartureSeconds = 3.2f;
        public const float ArrivalSeconds = 2.6f;
        public TravelPhase Phase { get; private set; }
        public float Elapsed { get; private set; }
        public bool ToIsland { get; private set; }
        public bool Active => Phase != TravelPhase.Idle;
        public bool Cinematic => Phase == TravelPhase.Departing || Phase == TravelPhase.Arriving;

        public bool Begin(bool toIsland)
        {
            if (Active) return false;
            ToIsland = toIsland;
            Elapsed = 0;
            Phase = TravelPhase.Departing;
            return true;
        }

        public TravelAction Advance(float seconds)
        {
            if (!Cinematic) return TravelAction.None;
            Elapsed += Math.Clamp(seconds, 0, 0.1f);
            if (Phase == TravelPhase.Departing && Elapsed >= DepartureSeconds)
                return Skip();
            if (Phase == TravelPhase.Arriving && Elapsed >= ArrivalSeconds)
                return Skip();
            return TravelAction.None;
        }

        public bool DestinationReady()
        {
            if (Phase != TravelPhase.Loading) return false;
            Phase = TravelPhase.Arriving;
            Elapsed = 0;
            return true;
        }

        public TravelAction Skip()
        {
            if (Phase == TravelPhase.Departing)
            {
                Phase = TravelPhase.Loading;
                Elapsed = 0;
                return TravelAction.LoadDestination;
            }
            if (Phase == TravelPhase.Arriving)
            {
                Reset();
                return TravelAction.Finish;
            }
            return TravelAction.None;
        }

        public void Reset() { Phase = TravelPhase.Idle; Elapsed = 0; }
    }
}
