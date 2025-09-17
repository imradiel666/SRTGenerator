import os.path
import argparse
import sys, codecs, locale
import warnings

import argostranslate.package, argostranslate.translate
#import argostranslatefiles
#from argostranslatefiles import argostranslatefiles

def runArgos(subtitle_file, from_code="ja", to_code="en"):
    # Download and install Argos Translate package
    argostranslate.package.update_package_index()
    available_packages = argostranslate.package.get_available_packages()
    package_to_install = next(
        filter(
            lambda x: x.from_code == from_code and x.to_code == to_code, available_packages
        )
    )
    argostranslate.package.install_from_path(package_to_install.download())

    installed_languages = argostranslate.translate.get_installed_languages()
    from_lang = list(filter(
        lambda x: x.code == from_code,
        installed_languages))[0]
    to_lang = list(filter(
        lambda x: x.code == to_code,
        installed_languages))[0]
    underlying_translation = from_lang.get_translation(to_lang)

    #argostranslatefiles.translate_file(underlying_translation, os.path.abspath(subtitle_file))    

def translateText(text: str, from_code: str = "ja", to_code: str = "en") -> str:
    # Download and install Argos Translate package
    argostranslate.package.update_package_index()
    available_packages = argostranslate.package.get_available_packages()
    package_to_install = next(
        filter(
            lambda x: x.from_code == from_code and x.to_code == to_code, available_packages
        )
    )
    argostranslate.package.install_from_path(package_to_install.download())

    # Load installed languages
    installed_languages = argostranslate.translate.get_installed_languages()

    # Select source and target languages
    from_lang = next((lang for lang in installed_languages if lang.code == from_code), None)
    to_lang = next((lang for lang in installed_languages if lang.code == to_code), None)

    if not from_lang or not to_lang:
        return "Language code not found or language not installed."

    # Get the translation model for the desired language pair
    translate = from_lang.get_translation(to_lang)

    # Translate text
    translated_text = translate.translate(text)
    print(translated_text)

    return translated_text

if __name__=="__main__":
    warnings.filterwarnings("ignore", "You are using `torch.load` with `weights_only=False`*.")

    # Initialize parser
    parser = argparse.ArgumentParser(description="Read subtitle file and pass to Argos translate.")
    parser.add_argument('-f', metavar='--SUBTITLE_FILE', help="Name of subtitle file.", required=False)
    parser.add_argument('-t', metavar='--TEXT', help="Text to translate.", required=False)
    parser.add_argument("-ch", metavar='--CACHE_HUB', help = "Set torch hub cache folder.", required=False)
    parser.add_argument('-from_lang', metavar='--FROM_LANG', help="Language text or file.", required=True)
    parser.add_argument('-to_lang', metavar='--TO_LANG', help="Language to translate text or file.", required=True)

    # Read arguments from command line
    args = parser.parse_args()

    # Settings:
    subtitle_path = args.f # @param {type:"string"}
    subtitle_text = args.t # @param {type:"string"}
    
    from_code = args.from_lang
    to_code = args.to_lang

    # @markdown Advanced settings:
    hubFolder = args.ch
    
    if hubFolder:
        torch.hub.set_dir(hubFolder)

    if subtitle_path:
        runArgos(subtitle_path, from_code, to_code)
    if subtitle_text:
        translatedText = translateText(subtitle_text)