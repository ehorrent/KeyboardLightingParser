use regex::Regex;
use std::collections::HashMap;
use std::fmt;

// -------------------------------------------------------------------------------------
// Public parser models

#[derive(Clone, Copy)]
pub enum Color {
  Green,
  Blue,
  Red,
  Yellow,
  Orange,
}

impl fmt::Display for Color {
  fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
    match self {
      Self::Green => write!(f, "green"),
      Self::Blue => write!(f, "blue"),
      Self::Red => write!(f, "red"),
      Self::Yellow => write!(f, "yellow"),
      Self::Orange => write!(f, "orange"),
    }
  }
}

pub enum KeyConfig {
  Static {
    code: String,
    color: Color,
  },
  Wave {
    code: String,
    colors: Vec<Color>,
  },
  Disco {
    code: String,
    color1: Color,
    color2: Color,
    color3: Color,
  },
}

impl KeyConfig {
  pub fn code(&self) -> &String {
    match self {
      Self::Static { code, .. } => code,
      Self::Wave { code, .. } => code,
      Self::Disco { code, .. } => code,
    }
  }
}

// -------------------------------------------------------------------------------------
// Private models

type InputLineToken = Vec<String>;

enum EffectToken {
  Static,
  Wave,
  Disco,
}

// -------------------------------------------------------------------------------------
// Private functions

/// Parse all supposed keys from a line input.
fn parse_keys(keys_line: &InputLineToken) -> Result<&InputLineToken, String> {
  let regex = Regex::new(r"[A-Za-z]").unwrap();

  let invalid_key = keys_line.iter().find(|key| false == regex.is_match(&key));
  match invalid_key {
    Some(key) => Err(format!("INVALID: Invalid key token: {}", key)),
    None => Ok(keys_line),
  }
}

/// Parse all supposed effects from a line.
fn parse_effects(effects_line: &InputLineToken) -> Result<EffectToken, String> {
  match effects_line.as_slice() {
    [effect] if effect == "static" => Ok(EffectToken::Static),
    [effect] if effect == "disco" => Ok(EffectToken::Disco),
    [effect] if effect == "wave" => Ok(EffectToken::Wave),
    [effect] => Err(format!("INVALID: Unknown effect: {}", effect)),
    _ => Err(String::from("INVALID: Only one effect expected by line")),
  }
}

/// Parse all supposed colors from a line.
fn parse_colors(colors_line: &InputLineToken) -> Result<Vec<Color>, String> {
  let mut colors = vec![];

  for color in colors_line {
    match color.as_str() {
      "green" => colors.push(Color::Green),
      "blue" => colors.push(Color::Blue),
      "red" => colors.push(Color::Red),
      "yellow" => colors.push(Color::Yellow),
      "orange" => colors.push(Color::Orange),
      _ => return Err(format!("INVALID: Invalid color: {}", color)),
    }
  }

  Ok(colors)
}

/// Parse a supposed effect from 3 lines.
/// Check the constraints for each effect type (Static / Wave / Disco).
fn parse_key_config(
  keys_line: &InputLineToken,
  effect_line: &InputLineToken,
  colors_line: &InputLineToken,
) -> Result<Vec<KeyConfig>, String> {
  let mut key_configs = vec![];
  let keys = parse_keys(keys_line)?;
  let effect = parse_effects(effect_line)?;
  let colors = parse_colors(colors_line)?;

  match effect {
    EffectToken::Wave => {
      for key in keys {
        key_configs.push(KeyConfig::Wave {
          code: String::from(key),
          colors: colors.clone(),
        });
      }
    }

    EffectToken::Static => match colors.as_slice() {
      [color] => {
        for key in keys {
          key_configs.push(KeyConfig::Static {
            code: String::from(key),
            color: *color,
          });
        }
      }

      _ => return Err(format!("INVALID: Static effects are single color only : ")),
    },

    EffectToken::Disco => match colors.as_slice() {
      [color1, color2, color3] => {
        for key in keys {
          key_configs.push(KeyConfig::Disco {
            code: String::from(key),
            color1: *color1,
            color2: *color2,
            color3: *color3,
          });
        }
      }

      _ => return Err(String::from("INVALID: Disco config requires 3 colors")),
    },
  }

  Ok(key_configs)
}

/// Parse all the input tokens and transform them into an ordered KeyConfig list.
fn parse_tokens(tokens: Vec<InputLineToken>) -> Result<Vec<KeyConfig>, String> {
  // This dictionary will cache all the key settings
  let mut all_key_configs = HashMap::new();

  if 0 != tokens.len() % 3 {
    return Err(String::from(
      "INVALID: Missing lines to complete the config",
    ));
  }

  for i in 0..tokens.len() / 3 {
    let keys_line = &tokens[3 * i];
    let effect_line = &tokens[3 * i + 1];
    let colors_line = &tokens[3 * i + 2];
    let key_configs = parse_key_config(keys_line, effect_line, colors_line)?;
    for key_config in key_configs {
      all_key_configs.insert(String::from(key_config.code()), key_config);
    }
  }

  let mut key_configs: Vec<KeyConfig> = all_key_configs
    .into_iter()
    .map(|key_value| key_value.1)
    .collect();

  key_configs.sort_by(|lhs, rhs| lhs.code().cmp(&rhs.code()));

  Ok(key_configs)
}

/// Transform a line into "clean" tokens:
/// - remove spaces and empty lines
/// - apply lower case
/// - split into tokens
fn tokenize(line: &str) -> InputLineToken {
  line
    .split(",")
    .map(|token| token.to_lowercase().trim().to_string())
    .filter(|value| value.len() > 0)
    .collect()
}

// -------------------------------------------------------------------------------------
// Public functions

/// Parse lines containing a keyboard lighting configuration.
pub fn parse(lines: Vec<&str>) -> Result<Vec<KeyConfig>, String> {
  let tokens = lines.into_iter().map(tokenize).collect();
  parse_tokens(tokens)
}

/// Parse a file containing a keyboard lighting configuration.
pub fn parse_file(filename: &str) -> Result<Vec<KeyConfig>, String> {
  let file_content = match std::fs::read_to_string(filename) {
    Err(err) => return Err(err.to_string()),
    Ok(content) => content,
  };

  let lines = file_content.lines().collect();
  parse(lines)
}
