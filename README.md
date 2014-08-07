Work Projects Installer Generator
=================================

All CodeUnit work projects use Advanced Installer 1.9 to build their installer files. Work Projects Installer Generator is a simple application that scans through all the work project folders in a specified base folder, searching for existing .aip installer files and then running an existing copy of Advanced Installer (on the system) in order to call a command line build operation. The generated files are then copied across into the project's main release folder.

Created by Craig Lotter, November 2007

*********************************

Project Details:

Coded in Visual Basic .NET using Visual Studio .NET 2005
Implements concepts such as file manipulation, text file manipulation, DOSShellCommand.
Level of Complexity: Very Simple
