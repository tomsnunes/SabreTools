<h2>SabreTools</h2>

<h3>Introduction</h3>

This project is a desktop port of some core features of Wizard of DATz Redux. For more information about the parent project, see <a href="https://github.com/mnadareski/wizzardRedux">here</a>.

<h3>Features</h3>

Currently, SabreTools consists of the following programs:

<b>SabreTools UI</b>
<p/>
A bare-bones attempt at providing a true GUI experience for the SabreTools suite. Currently only provides the following functions:
<ul>
	<li>Importing and Generating DAT files in RomVault and XML formats</li>
</ul>

<b>DATabase</b>
<p/>
The main tool of the SabreTools suite. Performs the majority of the core features of the parent project, including the following:
<ul>
	<li>Importing and Generating DAT files in RomVault and XML formats</li>
	<li>Converting DATs from RomVault to XML format</li>
	<li>Add and remove sources and systems from the database</li>
	<li>Retrieve a list of all sources and systems that are available</li>
</ul>
This tool can both be used with command line parameters or with the built-in text menu system.

<b>Headerer</b>
<p/>
A simple auxilary tool that detects and removes headers and also restores headers for the following systems:
<ul>
	<li>Atari 7800</li>
	<li>Atari Lynx</li>
	<li>NEC PC-Engine / TurboGrafx 16</li>
	<li>Nintendo Famicom Disk System</li>
	<li>Nintendo Entertainment System / Famicom</li>
	<li>Super Nintendo Entertainment System / Super Famicom</li>
</ul>
The saved headers are currently stored in a database; multiple headers allowed per file.

<b>DatSplit</b>
<p/>
This tool doesn't do everything possible for splitting DATs, most of those functions are taken care of by third-party tool <a href="http://www.logiqx.com/Tools/DatUtil/">DatUtil</a>.
<ul>
	<li>Split a DAT file using two different file extensions</li>
	<li>Works with RomVault and Logiqx XML DATs only because of current limitations</li>
</ul>
This is useful for splitting DAT files similar to the NonGood set that can have more than one system per DAT.

<b>DATFromDir</b>
<p/>
This a versitile Directory to DAT file creator developed to reduce the overhead of creating custom DAT files.
<ul>
	<li>Create a DIR from a file, a folder, or a set of either</li>
	<li>Set most of the DAT header values</li>
	<li>Enable DAT to be created with forcepacking="unzip"</li>
	<li>Treat archives like directories (default) or files</li>
	<li>Output to both supported DAT formats</li>
</ul>

<h3>Power User Tools</h3>

This section is for tools that have been requested for a highly specific purpose and are not generally going to be used by regular users.

<b>SingleGame</b>
<p/>
Experimental tool for trimming all files in DAT (or folder of DATs) so that they fall within the path size limits of NTFS. By default, it also merges all games into a single, minimally-named game for uncompressed sets.

<b>DatToMiss</b>
<p/>
CURRENTLY UNCODED - A program to convert a DAT to a miss file either for the games or the roms with the option to add a generic preface or postface to all of the outputted lines.

<b>MergeDAT</b>
<p/>
CURRENTLY UNCODED - A program to merge an arbitrary number of DATs into a single file and optionally dedupe the resulting set of roms.

<b>DiffDat</b>
<p/>
CURRENTLY UNCODED - A program to create a DAT based on the differences between two input DATs. Optionally, if the output file exists, the information is appended. (This latter functionality may be wrapped into MergeDAT.)

<h3>Licensing</h3>
<p/>
The preceeding programs use, in part or in whole, code, libraries, and/or applications from the <a href="www.7-zip.org">7-zip project</a>. 7-zip is licenced under the GNU LGPL.<br/>
All other external code is marked as such within the source and correctly attributed to the site and/or person(s) that originally wrote the code.
All code written by project members is licensed under GPL v3. See LICENSE for more details.

<h3>Contributions</h3>

Any feature suggestions or code contributions are accepted. Please take the time to look over the existing code to see proper coding and documentation patterns.