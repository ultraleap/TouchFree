export class AudioPlugin {
  audio?: HTMLAudioElement;

  public static instance: AudioPlugin;

  public static init = () => {
    if (!AudioPlugin.instance) {
      AudioPlugin.instance = new AudioPlugin();
    }
  };

  loadSound = (pathToSoundFile: string) => {
    this.audio = new Audio(pathToSoundFile);
    document.body.appendChild(this.audio);
  };

  playSound = () => {
    if (!this.audio) return;
    this.audio.play();
  };

  stopSound = () => {
    if (!this.audio) return;
    this.audio.pause();
    this.audio.load();
  };
}
