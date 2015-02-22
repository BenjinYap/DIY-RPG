using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Media;
using System.Xml;

namespace DIY_RPG {

    //This class does nothing except start the program and creates the Main class
    public class Program {

        static void Main (string[] args) {
            XmlDocument awd = new XmlDocument ();
            awd.Load ("RPGs\\Pokemals.xml");
            new PreGameScreen (awd);
            //new DIYRPGScreen ();
        }
    }
}
