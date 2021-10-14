####################################################################################
|                                                                                  |
|          ******                                                  **              |
|         **////**                                         **   **//*              |
|        **    //   ******  *******  ***     **  ******   //** **  /   ******      |
|       /**        **////**//**///**//**  * /** //////**   //***      **////       |
|       /**       /**   /** /**  /** /** ***/**  *******    /**      //*****       |
|       //**    **/**   /** /**  /** /****/**** **////**    **        /////**      |
|        //****** //******  ***  /** ***/ ///**//********  **         ******       |
|         //////   //////  ///   // ///    ///  ////////  //         //////        |
|              ********                                             ****           |
|             **//////**                                           /**/            |
|            **      //   ******   **********   *****     ******  ******           |
|           /**          //////** //**//**//** **///**   **////**///**/            |
|           /**    *****  *******  /** /** /**/*******  /**   /**  /**             |
|           //**  ////** **////**  /** /** /**/**////   /**   /**  /**             |
|            //******** //******** *** /** /**//******  //******   /**             |
|             ////////   //////// ///  //  //  //////    //////    //              |
|                             **       **   ****                                   |
|                            /**      //   /**/                                    |
|                            /**       ** ******  *****                            |
|                            /**      /**///**/  **///**                           |
|                            /**      /**  /**  /*******                           |
|                            /**      /**  /**  /**////                            |
|                            /********/**  /**  //******                           |
|                            //////// //   //    //////                            |
|                                                                                  |
####################################################################################

Greetings! Rob Sale here with an exciting submission :D

Along with the fact that this program was written in UWP, there have been some
additional enhancements which sets this apart from the rubric which I am hoping to
have included in the grading process.

------------------------------------------------------------------------------------

  1. Import directly from the Lexicon

This feature is a big one. I reverse engineered the GoL Lexicon website and
discovered when you click on a link it takes you to an interactive GoL running
with WASM, and in the payload is a json object containing the plaintext format
of the initial universe. I used a webview in my app with a "Navigation Changed"
event handler to see which universe the user clicks, and subsequently acquires
the payload so it can be imported directly into the universe.

Usage: File -> Import -> From Web

  2: Previous button

It's fairly simple and I might not keep this button moving forward, but it's been 
helpful in quickly observing patterns. 

  3: Drag select cells - WIP

This app will feature the ability to select a group of cells, cut/copy and paste 
them so you can quickly build out repetitious patterns with minimal effort