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

  2: 