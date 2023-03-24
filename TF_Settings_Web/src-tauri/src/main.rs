
#![cfg_attr(
  all(not(debug_assertions), target_os = "windows"),
  windows_subsystem = "windows"
)]

fn main() {
  tauri::Builder::default()
  .invoke_handler(tauri::generate_handler![read_file_to_string, write_string_to_file])
  .run(tauri::generate_context!())
  .expect("error while running tauri application");
}

use std::fs;
#[tauri::command]
fn read_file_to_string(path: String) -> Result<String, String> {
  let contents = fs::read_to_string(path).map_err(|err| err.to_string())?;
  Ok(contents.into())
}

#[tauri::command]
fn write_string_to_file(path: String, contents: String) -> Result<(), String> {
  fs::write(path, contents).map_err(|err| err.to_string())?;
  Ok(())
}