#!/usr/bin/env python3
"""Original synthesized Mermaid Queen SFX. Requires numpy; writes mono 44.1kHz PCM WAV.
No recordings, sampled game audio, AI voice or external copyrighted sources.
"""
from pathlib import Path
import wave
import numpy as np

RATE = 44100
OUT = Path(__file__).resolve().parents[1] / 'Assets/Sounds/Queen'
OUT.mkdir(parents=True, exist_ok=True)
rng = np.random.default_rng(120226)

def time(seconds):
    return np.arange(round(seconds * RATE)) / RATE

def envelope(t, attack=.02, release=.12):
    return np.minimum(t / attack, 1) * np.minimum((t[-1] - t) / release, 1)

def noise(t, low=200, high=7000):
    raw = rng.standard_normal(len(t))
    f = np.fft.rfftfreq(len(t), 1 / RATE)
    mask = np.minimum(f / max(low, 1), 1)**3 / (1 + (f / high)**6)
    out = np.fft.irfft(np.fft.rfft(raw) * mask, n=len(t))
    return out / max(np.std(out), .001)

def chime(t, frequency, decay=5):
    return (np.sin(2*np.pi*frequency*t) + .23*np.sin(2*np.pi*frequency*2.76*t)) * np.exp(-decay*t)

def voice(t, notes, vowel=(700, 1200, 2600)):
    # Harmonic resonators approximate an airy vowel; this is deliberately a
    # stylized synth siren, not a claim of a recorded/natural female singer.
    pitch = np.interp(t, np.linspace(0, t[-1], len(notes)), notes)
    pitch *= 1 + .008*np.sin(2*np.pi*5.2*t)
    phase = np.cumsum(pitch) * 2*np.pi/RATE
    out = np.zeros(len(t))
    for harmonic in range(1, 42):
        freq = pitch*harmonic
        gain = .05 / harmonic + sum(np.exp(-.5*((freq-center)/width)**2)*amp for center,width,amp in zip(vowel,(100,150,230),(1,.55,.2))) / harmonic**.45
        out += gain*np.sin(phase*harmonic)
    return out*.6 + noise(t, 1500, 5000)*.025

def echo(data, taps):
    result=data.copy()
    for delay,gain in taps:
        offset=round(delay*RATE)
        if offset<len(data): result[offset:] += data[:-offset]*gain
    return result

def write(name, data):
    data -= np.mean(data)
    data *= envelope(np.arange(len(data))/RATE, .008, .035)
    peak=np.max(np.abs(data))
    data=data/max(peak,.001)*.78
    assert np.isfinite(data).all()
    pcm=np.round(data*32767).astype('<i2')
    with wave.open(str(OUT/f'{name}.wav'),'wb') as f:
        f.setparams((1,2,RATE,0,'NONE','not compressed')); f.writeframes(pcm.tobytes())
    print(f'{name}: {len(data)/RATE:.2f}s, peak {max(abs(pcm.astype(int)))/32767:.3f}')

t=time(2.0);write('Awaken',echo(voice(t,[261.6,329.6,392,440])*envelope(t,.22,.65)+.12*chime(t,1568,2),[(.17,.3),(.31,.15)]))
t=time(.75);write('DashCharge',echo((.28*noise(t,900,5500)+np.sin(2*np.pi*(220*t+330*t*t))*.3)*envelope(t,.55,.08),[(.09,.16)]))
t=time(.42);write('Dash',(.65*noise(t,500,6500)+.18*np.sin(2*np.pi*(280*t-200*t*t)))*envelope(t,.012,.22)*np.exp(-3*t))
t=time(.8);write('Dive',echo((noise(t,180,4000)*.55+np.sin(2*np.pi*(190*t-80*t*t))*.3)*envelope(t,.015,.5),[(.08,.2),(.18,.1)]))
t=time(.72);write('Wave',(noise(t,80,2400)*(.6+.18*np.sin(t*36))+noise(t,1800,7000)*.18)*envelope(t,.12,.38))
t=time(.65);write('Emerge',echo((noise(t,900,6500)*.5+.1*chime(t,1174,7))*envelope(t,.015,.4),[(.12,.18)]))
t=time(1.2);write('Summon',echo((voice(t,[196,220,261.6],(430,900,2100))*.7+.13*chime(t,784,2))*envelope(t,.12,.4),[(.16,.25),(.29,.1)]))
t=time(2.35);write('Song',echo((voice(t,[329.6,349.2,392,440,440,392])+.08*np.sin(2*np.pi*880*t))*envelope(t,.18,.75),[(.13,.22),(.27,.12)]))
t=time(.6);write('SongRelease',echo((chime(t,1318.5,6)*.4+chime(t,1760,7)*.22+noise(t,2200,7000)*.14)*envelope(t,.01,.25),[(.12,.2)]))
t=time(.24);write('Hurt',(voice(t,[523,392])*.5+noise(t,700,3500)*.15)*envelope(t,.008,.15))
t=time(1.85);write('Death',echo((voice(t,[440,392,293.7,196])*.8+noise(t,300,5000)*.14+chime(t,2093,3)*.1)*envelope(t,.03,1),[(.19,.3),(.37,.18)]))
