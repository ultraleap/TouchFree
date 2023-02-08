
#![cfg_attr(
  all(not(debug_assertions), target_os = "windows"),
  windows_subsystem = "windows"
)]

fn main() {
  tauri::Builder::default()
  .invoke_handler(tauri::generate_handler![read_visuals_config])
  .run(tauri::generate_context!())
  .expect("error while running tauri application");
}

use std::fs::File;
use std::io::Read;
#[tauri::command]
fn read_visuals_config() -> String {
  let mut file = File::open("C:/ProgramData/Ultraleap/TouchFree/Configuration/TouchFreeConfig.json").expect("Unable to open TouchFreeConfig.json");
  let mut contents = String::new();
  file.read_to_string(&mut contents).expect("Unable to read TouchFreeConfig.json");
  contents.into()
}