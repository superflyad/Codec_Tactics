using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Audio;

namespace CodecTactics.MonoGame;

public sealed class AudioService : IDisposable
{
    private readonly Dictionary<AudioCue, SoundEffect> _effects = new();
    private SoundEffectInstance _ambient;
    private bool _disposed;

    public void Load(string contentRoot)
    {
        var audioRoot = Path.Combine(contentRoot, "Audio");
        LoadCue(audioRoot, AudioCue.Hover, "hover.wav");
        LoadCue(audioRoot, AudioCue.Select, "select.wav");
        LoadCue(audioRoot, AudioCue.Confirm, "confirm.wav");
        LoadCue(audioRoot, AudioCue.Invalid, "invalid.wav");
        LoadCue(audioRoot, AudioCue.Capture, "capture.wav");
        LoadCue(audioRoot, AudioCue.Reinforce, "reinforce.wav");
        LoadCue(audioRoot, AudioCue.Weaken, "weaken.wav");
        LoadCue(audioRoot, AudioCue.Corruption, "corruption.wav");
        LoadCue(audioRoot, AudioCue.Objective, "objective.wav");
        LoadCue(audioRoot, AudioCue.Victory, "victory.wav");
        LoadCue(audioRoot, AudioCue.Defeat, "defeat.wav");
        LoadCue(audioRoot, AudioCue.Reset, "reset.wav");

        var ambientPath = Path.Combine(audioRoot, "ambient-network-hum.wav");
        if (File.Exists(ambientPath))
        {
            using var stream = File.OpenRead(ambientPath);
            _ambient = SoundEffect.FromStream(stream).CreateInstance();
            _ambient.IsLooped = true;
            _ambient.Volume = 0.22f;
            _ambient.Play();
        }
    }

    public void Play(AudioCue cue, float volume = 0.7f)
    {
        if (_effects.TryGetValue(cue, out var effect))
        {
            effect.Play(Math.Clamp(volume, 0f, 1f), 0f, 0f);
        }
    }

    public void SetAmbientIntensity(float intensity)
    {
        if (_ambient is null)
        {
            return;
        }

        intensity = Math.Clamp(intensity, 0f, 1f);
        _ambient.Volume = 0.18f + intensity * 0.18f;
        _ambient.Pitch = -0.08f + intensity * 0.14f;
        _ambient.Pan = MathF.Sin(Environment.TickCount / 4200f) * 0.08f;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _ambient?.Stop();
        _ambient?.Dispose();
        foreach (var effect in _effects.Values)
        {
            effect.Dispose();
        }

        _disposed = true;
    }

    private void LoadCue(string audioRoot, AudioCue cue, string fileName)
    {
        var path = Path.Combine(audioRoot, fileName);
        if (!File.Exists(path))
        {
            return;
        }

        using var stream = File.OpenRead(path);
        _effects[cue] = SoundEffect.FromStream(stream);
    }
}
