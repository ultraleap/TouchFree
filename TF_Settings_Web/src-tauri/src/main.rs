
#![cfg_attr(
  all(not(debug_assertions), target_os = "windows"),
  windows_subsystem = "windows"
)]

fn main() {
  tauri::Builder::default()
  .invoke_handler(tauri::generate_handler![read_file_to_string])
  .run(tauri::generate_context!())
  .expect("error while running tauri application");
}

use std::fs::File;
use std::io::Read;
#[tauri::command]
fn read_file_to_string(path: String) -> Result<String, String> {
  let mut file = File::open(path).map_err(|err| err.to_string())?;
  let mut contents = String::new();
  file.read_to_string(&mut contents).map_err(|err| err.to_string())?;
  Ok(contents.into())
}