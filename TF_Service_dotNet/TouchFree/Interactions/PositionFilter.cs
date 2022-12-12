using Microsoft.Extensions.Options;
using System;
using System.Numerics;

namespace Ultraleap.TouchFree.Library.Interactions
{
    public class PositionFilter : IPositionModifier
    {
        private bool enabled = false;
        private bool initialised = false;
        private Vector2 lastPosition;

        private readonly float dcutoff;
        private readonly float mincutoff;

        private readonly float beta;

        private readonly Filter dxFilter = new Filter();
        private readonly Filter dyFilter = new Filter();
        private readonly Filter xFilter = new Filter();
        private readonly Filter yFilter = new Filter();

        public PositionFilter(IOptions<InteractionTuning> _interactionTuning)
        {
            beta = 0.1f;
            dcutoff = 0.5f;
            mincutoff = 0.5f;
            enabled = _interactionTuning?.Value?.EnableOneEuroFilter ?? false;
        }

        public Vector2 ApplyModification(Vector2 position)
        {
            if (!enabled)
            {
                return position;
            }

            if (!initialised)
            {
                lastPosition = position;
                initialised = true;
                return lastPosition;
            }
            else
            {
                var dPosition = position - lastPosition;
                var filteredDx = Math.Abs(dxFilter.FilterValue(dPosition.X, CalculateAlpha(dcutoff)));
                var x = xFilter.FilterValue(position.X, CalculateAlpha(mincutoff + beta * filteredDx));

                var filteredDy = Math.Abs(dyFilter.FilterValue(dPosition.Y, CalculateAlpha(dcutoff)));
                var y = yFilter.FilterValue(position.Y, CalculateAlpha(mincutoff + beta * filteredDy));

                lastPosition = position;
                return new Vector2(x, y);
            }
        }

        private static float CalculateAlpha(float cutOff)
        {
            return 1f / (1f + (60f / (float)(2f * Math.PI * cutOff)));
        }

        private class Filter
        {
            private float lastValue;
            private float lastFilteredValue;
            private bool initialised = false;

            public Filter()
            {
                initialised = false;
            }

            public float FilterValue(float value, float alpha)
            {
                if (initialised)
                {
                    lastFilteredValue = alpha * value + (1f - alpha) * lastFilteredValue;
                }
                else
                {
                    lastFilteredValue = value;
                    initialised = true;
                }
                lastValue = value;

                return lastFilteredValue;
            }

            public float LastValue()
            {
                return lastValue;
            }
        }
    }
}