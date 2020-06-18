mod parser;
use parser::*;

/// Display the result of the parsing
fn display_result(parse_result: &Result<Vec<KeyConfig>, String>) {
    match parse_result {
        Err(error) => println!("Error: {}", error),

        Ok(key_configs) => {
            for key_config in key_configs {
                match key_config {
                    KeyConfig::Static { code, color } => {
                        println!("{}, {}, {}", code, "static", color)
                    }
                    KeyConfig::Wave { code, colors } => {
                        let str_colors: Vec<String> =
                            colors.iter().map(|c| c.to_string()).collect();
                        println!("{}, {}, [{}]", code, "wave", str_colors.join("-"))
                    }
                    KeyConfig::Disco {
                        code,
                        color1,
                        color2,
                        color3,
                    } => println!("{}, {}, {}, {}, {}", code, "disco", color1, color2, color3),
                }
            }
        }
    }
}

pub fn main() {
    let args: Vec<String> = std::env::args().collect();
    match args.as_slice() {
        [_, filename] => {
            let parse_result = parse_file(filename);
            display_result(&parse_result);
        }
        _ => println!("ERROR: Please provide an input file as argument"),
    }
}
