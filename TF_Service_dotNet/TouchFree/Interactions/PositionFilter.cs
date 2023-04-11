using Microsoft.Extensions.Options;
using System;
using System.Numerics;

namespace Ultraleap.TouchFree.Library.Interactions;

public class PositionFilter : IPositionModifier
{
    private bool _enabled = false;
    private bool _initialised = false;
    private Vector2 _lastPosition;

    private readonly float _dcutoff;
    private readonly float _mincutoff;

    private readonly float _beta;

    private readonly Filter _dxFilter = new();
    private readonly Filter _dyFilter = new();
    private readonly Filter _xFilter = new();
    private readonly Filter _yFilter = new();

    public PositionFilter(IOptions<InteractionTuning> interactionTuning)
    {
        _beta = 0.1f;
        _dcutoff = 0.5f;
        _mincutoff = 0.5f;
        _enabled = interactionTuning?.Value?.EnableOneEuroFilter ?? false;
    }

    public Vector2 ApplyModification(Vector2 position)
    {
        if (!_enabled)
        {
            return position;
        }

        if (!_initialised)
        {
            _lastPosition = position;
            _initialised = true;
            return _lastPosition;
        }
        else
        {
            var dPosition = position - _lastPosition;
            var filteredDx = Math.Abs(_dxFilter.FilterValue(dPosition.X, CalculateAlpha(_dcutoff)));
            var x = _xFilter.FilterValue(position.X, CalculateAlpha(_mincutoff + _beta * filteredDx));

            var filteredDy = Math.Abs(_dyFilter.FilterValue(dPosition.Y, CalculateAlpha(_dcutoff)));
            var y = _yFilter.FilterValue(position.Y, CalculateAlpha(_mincutoff + _beta * filteredDy));

            _lastPosition = position;
            return new Vector2(x, y);
        }
    }

    private static float CalculateAlpha(float cutOff) => 1f / (1f + (60f / (float)(2f * Math.PI * cutOff)));

    private class Filter
    {
        private float _lastFilteredValue;
        private bool _initialised = false;

        public Filter()
        {
            _initialised = false;
        }

        public float FilterValue(float value, float alpha)
        {
            if (_initialised)
            {
                _lastFilteredValue = alpha * value + (1f - alpha) * _lastFilteredValue;
            }
            else
            {
                _lastFilteredValue = value;
                _initialised = true;
            }

            return _lastFilteredValue;
        }
    }
}