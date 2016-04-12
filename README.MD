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

<b>SingleGame</b>
<p/>
Hightly experimental tool for merging all games in a single DAT (RomVault or Loqiqx XML only) for use with a server environment. Requested by Kludge.

<b>DATFromDir</b>
<p/>
This a versitile Directory to DAT file creator developed to reduce the overhead of creating custom DAT files.
<ul>
	<li>Create a DIR from a file, a folder, or a set of either</li>
	<li>Set most of the DAT header values</li>
	<li>Enable DAT to be created with forcepacking="unzip"</li>
	<li>Treat archives like directories (default) or files</li>
	<li>Output to XML DAT</li>
</ul>

<h3>Contributions</h3>

Any feature suggestions or code contributions are accepted. Please take the time to look over the existing code to see proper coding and documentation patterns.