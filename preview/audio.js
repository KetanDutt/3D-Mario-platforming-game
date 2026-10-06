// Web Audio API Procedural Synthesizer for 3D Mario Sound Effects & BGM

class SoundEngine {
    constructor() {
        this.ctx = null;
        this.masterGain = null;
        this.sfxGain = null;
        this.bgmGain = null;
        this.muted = false;
        this.bgmPlaying = false;
        this.bgmInterval = null;
        this.bgmType = 'overworld'; // 'overworld' or 'underground'
    }

    init() {
        if (this.ctx) return;
        const AudioContext = window.AudioContext || window.webkitAudioContext;
        this.ctx = new AudioContext();

        this.masterGain = this.ctx.createGain();
        this.masterGain.gain.value = 0.8;
        this.masterGain.connect(this.ctx.destination);

        this.sfxGain = this.ctx.createGain();
        this.sfxGain.gain.value = 0.9;
        this.sfxGain.connect(this.masterGain);

        this.bgmGain = this.ctx.createGain();
        this.bgmGain.gain.value = 0.35;
        this.bgmGain.connect(this.masterGain);
    }

    resume() {
        if (!this.ctx) this.init();
        if (this.ctx.state === 'suspended') {
            this.ctx.resume();
        }
    }

    setMasterVolume(val) {
        if (this.masterGain) this.masterGain.gain.value = val;
    }

    playJump(count = 1) {
        this.resume();
        if (!this.ctx) return;

        const now = this.ctx.currentTime;
        const osc = this.ctx.createOscillator();
        const gain = this.ctx.createGain();

        osc.type = 'square';
        const baseFreq = count === 3 ? 380 : (count === 2 ? 300 : 220);
        const endFreq = baseFreq * 2.2;

        osc.frequency.setValueAtTime(baseFreq, now);
        osc.frequency.exponentialRampToValueAtTime(endFreq, now + 0.18);

        gain.gain.setValueAtTime(0.3, now);
        gain.gain.exponentialRampToValueAtTime(0.01, now + 0.22);

        osc.connect(gain);
        gain.connect(this.sfxGain);

        osc.start(now);
        osc.stop(now + 0.22);
    }

    playCoin() {
        this.resume();
        if (!this.ctx) return;

        const now = this.ctx.currentTime;
        const osc = this.ctx.createOscillator();
        const gain = this.ctx.createGain();

        osc.type = 'sine';
        osc.frequency.setValueAtTime(987.77, now); // B5
        osc.frequency.setValueAtTime(1318.51, now + 0.08); // E6

        gain.gain.setValueAtTime(0.4, now);
        gain.gain.setValueAtTime(0.4, now + 0.08);
        gain.gain.exponentialRampToValueAtTime(0.001, now + 0.5);

        osc.connect(gain);
        gain.connect(this.sfxGain);

        osc.start(now);
        osc.stop(now + 0.5);
    }

    playStomp() {
        this.resume();
        if (!this.ctx) return;

        const now = this.ctx.currentTime;
        const osc = this.ctx.createOscillator();
        const gain = this.ctx.createGain();

        osc.type = 'triangle';
        osc.frequency.setValueAtTime(280, now);
        osc.frequency.exponentialRampToValueAtTime(45, now + 0.15);

        gain.gain.setValueAtTime(0.5, now);
        gain.gain.exponentialRampToValueAtTime(0.01, now + 0.18);

        osc.connect(gain);
        gain.connect(this.sfxGain);

        osc.start(now);
        osc.stop(now + 0.18);
    }

    playKick() {
        this.resume();
        if (!this.ctx) return;

        const now = this.ctx.currentTime;
        const osc = this.ctx.createOscillator();
        const gain = this.ctx.createGain();

        osc.type = 'triangle';
        osc.frequency.setValueAtTime(350, now);
        osc.frequency.exponentialRampToValueAtTime(70, now + 0.14);

        gain.gain.setValueAtTime(0.6, now);
        gain.gain.exponentialRampToValueAtTime(0.01, now + 0.16);

        osc.connect(gain);
        gain.connect(this.sfxGain);

        osc.start(now);
        osc.stop(now + 0.16);
    }

    playBlockBump() {
        this.resume();
        if (!this.ctx) return;

        const now = this.ctx.currentTime;
        const osc = this.ctx.createOscillator();
        const gain = this.ctx.createGain();

        osc.type = 'square';
        osc.frequency.setValueAtTime(180, now);
        osc.frequency.exponentialRampToValueAtTime(80, now + 0.1);

        gain.gain.setValueAtTime(0.35, now);
        gain.gain.exponentialRampToValueAtTime(0.01, now + 0.12);

        osc.connect(gain);
        gain.connect(this.sfxGain);

        osc.start(now);
        osc.stop(now + 0.12);
    }

    playBrickBreak() {
        this.resume();
        if (!this.ctx) return;

        const now = this.ctx.currentTime;
        // White noise burst
        const bufferSize = this.ctx.sampleRate * 0.2;
        const buffer = this.ctx.createBuffer(1, bufferSize, this.ctx.sampleRate);
        const data = buffer.getChannelData(0);
        for (let i = 0; i < bufferSize; i++) {
            data[i] = (Math.random() * 2 - 1) * Math.exp(-i / (bufferSize * 0.3));
        }

        const noise = this.ctx.createBufferSource();
        noise.buffer = buffer;

        const filter = this.ctx.createBiquadFilter();
        filter.type = 'lowpass';
        filter.frequency.setValueAtTime(800, now);
        filter.frequency.exponentialRampToValueAtTime(150, now + 0.2);

        const gain = this.ctx.createGain();
        gain.gain.setValueAtTime(0.5, now);
        gain.gain.exponentialRampToValueAtTime(0.01, now + 0.2);

        noise.connect(filter);
        filter.connect(gain);
        gain.connect(this.sfxGain);

        noise.start(now);
    }

    playFireball() {
        this.resume();
        if (!this.ctx) return;

        const now = this.ctx.currentTime;
        const osc = this.ctx.createOscillator();
        const gain = this.ctx.createGain();

        osc.type = 'triangle';
        osc.frequency.setValueAtTime(800, now);
        osc.frequency.exponentialRampToValueAtTime(200, now + 0.12);

        gain.gain.setValueAtTime(0.4, now);
        gain.gain.exponentialRampToValueAtTime(0.01, now + 0.12);

        osc.connect(gain);
        gain.connect(this.sfxGain);

        osc.start(now);
        osc.stop(now + 0.12);
    }

    playPowerup() {
        this.resume();
        if (!this.ctx) return;

        const notes = [330, 392, 659, 523, 587, 784];
        const stepTime = 0.07;
        notes.forEach((freq, i) => {
            const now = this.ctx.currentTime + i * stepTime;
            const osc = this.ctx.createOscillator();
            const gain = this.ctx.createGain();

            osc.type = 'triangle';
            osc.frequency.setValueAtTime(freq, now);

            gain.gain.setValueAtTime(0.3, now);
            gain.gain.exponentialRampToValueAtTime(0.01, now + stepTime * 1.2);

            osc.connect(gain);
            gain.connect(this.sfxGain);

            osc.start(now);
            osc.stop(now + stepTime * 1.2);
        });
    }

    play1Up() {
        this.resume();
        if (!this.ctx) return;

        const notes = [330, 392, 659, 523, 587, 784, 880, 1046.5];
        const stepTime = 0.06;
        notes.forEach((freq, i) => {
            const now = this.ctx.currentTime + i * stepTime;
            const osc = this.ctx.createOscillator();
            const gain = this.ctx.createGain();

            osc.type = 'sine';
            osc.frequency.setValueAtTime(freq, now);

            gain.gain.setValueAtTime(0.35, now);
            gain.gain.exponentialRampToValueAtTime(0.01, now + stepTime * 1.3);

            osc.connect(gain);
            gain.connect(this.sfxGain);

            osc.start(now);
            osc.stop(now + stepTime * 1.3);
        });
    }

    playPipe() {
        this.resume();
        if (!this.ctx) return;

        const steps = [400, 300, 250, 180, 140, 110];
        const stepTime = 0.05;
        steps.forEach((freq, i) => {
            const now = this.ctx.currentTime + i * stepTime;
            const osc = this.ctx.createOscillator();
            const gain = this.ctx.createGain();

            osc.type = 'sawtooth';
            osc.frequency.setValueAtTime(freq, now);

            gain.gain.setValueAtTime(0.35, now);
            gain.gain.exponentialRampToValueAtTime(0.01, now + stepTime);

            osc.connect(gain);
            gain.connect(this.sfxGain);

            osc.start(now);
            osc.stop(now + stepTime);
        });
    }

    playFlagpole() {
        this.resume();
        if (!this.ctx) return;

        // Slide down sound
        const now = this.ctx.currentTime;
        const osc = this.ctx.createOscillator();
        const gain = this.ctx.createGain();

        osc.type = 'triangle';
        osc.frequency.setValueAtTime(800, now);
        osc.frequency.exponentialRampToValueAtTime(120, now + 1.2);

        gain.gain.setValueAtTime(0.3, now);
        gain.gain.exponentialRampToValueAtTime(0.01, now + 1.2);

        osc.connect(gain);
        gain.connect(this.sfxGain);

        osc.start(now);
        osc.stop(now + 1.2);

        // Fanfare melody after 1.2s
        const fanfareNotes = [
            { f: 523.25, d: 0.15 }, { f: 659.25, d: 0.15 }, { f: 783.99, d: 0.15 },
            { f: 1046.50, d: 0.35 }, { f: 880.00, d: 0.2 }, { f: 1046.50, d: 0.6 }
        ];

        let offset = 1.3;
        fanfareNotes.forEach((note) => {
            const noteTime = now + offset;
            const fOsc = this.ctx.createOscillator();
            const fGain = this.ctx.createGain();

            fOsc.type = 'triangle';
            fOsc.frequency.setValueAtTime(note.f, noteTime);

            fGain.gain.setValueAtTime(0.4, noteTime);
            fGain.gain.exponentialRampToValueAtTime(0.01, noteTime + note.d);

            fOsc.connect(fGain);
            fGain.connect(this.sfxGain);

            fOsc.start(noteTime);
            fOsc.stop(noteTime + note.d);

            offset += note.d + 0.05;
        });
    }

    playHurt() {
        this.resume();
        if (!this.ctx) return;

        const now = this.ctx.currentTime;
        const osc = this.ctx.createOscillator();
        const gain = this.ctx.createGain();

        osc.type = 'sawtooth';
        osc.frequency.setValueAtTime(320, now);
        osc.frequency.exponentialRampToValueAtTime(90, now + 0.3);

        gain.gain.setValueAtTime(0.4, now);
        gain.gain.exponentialRampToValueAtTime(0.01, now + 0.35);

        osc.connect(gain);
        gain.connect(this.sfxGain);

        osc.start(now);
        osc.stop(now + 0.35);
    }

    startBGM(type = 'overworld') {
        this.resume();
        this.bgmType = type;
        if (this.bgmPlaying) return;
        this.bgmPlaying = true;

        // Upbeat Super Bell Hill inspired sequence
        const overworldMelody = [
            { f: 659.25, d: 0.16 }, { f: 659.25, d: 0.16 }, { f: 0, d: 0.16 }, { f: 659.25, d: 0.16 },
            { f: 0, d: 0.16 }, { f: 523.25, d: 0.16 }, { f: 659.25, d: 0.22 }, { f: 783.99, d: 0.35 },
            { f: 0, d: 0.2 }, { f: 392.00, d: 0.35 }, { f: 0, d: 0.25 },
            { f: 523.25, d: 0.22 }, { f: 392.00, d: 0.22 }, { f: 329.63, d: 0.22 },
            { f: 440.00, d: 0.22 }, { f: 493.88, d: 0.22 }, { f: 466.16, d: 0.16 }, { f: 440.00, d: 0.22 },
            { f: 392.00, d: 0.22 }, { f: 659.25, d: 0.22 }, { f: 783.99, d: 0.22 }, { f: 880.00, d: 0.3 }
        ];

        let noteIdx = 0;
        const playNext = () => {
            if (!this.bgmPlaying || !this.ctx) return;
            const note = overworldMelody[noteIdx];
            noteIdx = (noteIdx + 1) % overworldMelody.length;

            if (note.f > 0) {
                const now = this.ctx.currentTime;
                const osc = this.ctx.createOscillator();
                const gain = this.ctx.createGain();

                osc.type = 'triangle';
                osc.frequency.setValueAtTime(note.f, now);

                gain.gain.setValueAtTime(0.2, now);
                gain.gain.exponentialRampToValueAtTime(0.01, now + note.d);

                osc.connect(gain);
                gain.connect(this.bgmGain);

                osc.start(now);
                osc.stop(now + note.d);
            }

            this.bgmTimeout = setTimeout(playNext, (note.d + 0.08) * 1000);
        };

        playNext();
    }

    stopBGM() {
        this.bgmPlaying = false;
        if (this.bgmTimeout) clearTimeout(this.bgmTimeout);
    }
}

window.soundEngine = new SoundEngine();
