# OSMToSCT
Simple console application for converting OSM files to region description text for use in .sct2 sector files.

OSMToSCT is used to quickly generate regions for .sct2 sector files for use in the VATSIM Virtual Radar Client (VRC).

To use OSMToSCT, you can either run it with a single command line argument indicating a path to the .osm file (or a directory of multiple .osm files) to process, or you can run it without any arguments, in which case it will prompt for the input path.
The application will then generate a .txt file for each input file containing the region description text. You can then copy and paste this text into a sector file as needed.
