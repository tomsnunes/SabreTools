<h2>SabreTools</h2>

<h3>Introduction</h3>

This project started as a desktop port of some core features of Wizard of DATz Redux, but it has since grown into a fully-featured DAT management tool. Below is a brief description of the features available in the suite. For more information about each feature, see the help text in each of the programs. For more information about the parent project, see <a href="https://github.com/mnadareski/wizzardRedux">here</a>.

<h3>Features</h3>

Currently, the SabreTools suite consists of the following programs:

<!--
<b>SabreTools UI</b>
<p/>
A bare-bones attempt at providing a true GUI experience for the SabreTools suite. Currently only provides the following functions:
<ul>
	<li>Importing and Generating DAT files in ClrMamePro and XML formats</li>
</ul>
This tool is not recommended for any usage.
-->

<b>SabreTools</b> (formerly <i>DATabase</i>)
<p/>
The main tool of the SabreTools suite. Performs the majority of the core features of the parent project, including the following:
<ul>
	<li>Importing and Generating DAT files in ClrMamePro and XML formats (current version formerly <i>DATabaseTwo</i>)</li>
	<li>Create a DAT from a file, a folder, or a set of either(formerly <i>DATFromDir</i>)</li>
	<li>Converting DATs from any format to ClrMamePro, Logiqx XML, SabreDAT XML, and to missfile (last part formerly <i>DatToMiss</i>; requested by Obiwantje)</li>
	<li>Trim DAT entries and optionally merge into a single game (formerly <i>SingleGame</i>; requested by Kludge)</li>
	<li>Split a DAT using two different file extensions within the DAT (formerly <i>DatSplit</i>)</li>
	<li>Split a DAT or folder of DATs by best available hash or nodump (formerly <i>HashSplit</i>, requested by Obiwantje)</li>
	<li>Merge, diff, and dedupe an arbitrary number of DAT files, as long as it's two or more (formerly <i>MergeDAT</i>; requested by Obiwantje and Kludge)
		<ul>
			<li>Read in from commandline, drag-and-drop, a directory, or any combination</li>
			<li>Roms are compared with all information available, so duplicates can exist in the output</li>
			<li>Output type automatically determined by the first input file</li>
			<li>Optionally create a DiffDat of the inputted files instead of a straight merge</li>
			<li>Optionally merge the output DAT so no partial duplicates are included</li>
		</ul>
	</li>
	<li>Get various statistics on a DAT or folder of DATs including number of roms, disks, and even total size in a human readable format (formerly <i>UncompressedSize</i>; additions requested by Obiwantje)</li>
	<li>Filter a DAT by any criteria that the user chooses including wildcard searches (formerly <i>Filter</i>, requested by Obiwantje and others)</li>
	<li>Take a current merged DAT, a current missing DAT, and a new merged DAT and get all new files, files to remove, and a new missing DAT. Optionally set all hash values to 0-byte equivalents for rom managers (formerly <i>OfflineMerge</i>, requested by Obiwantje)</li>
	<li>Add and remove sources and systems from the database</li>
	<li>Retrieve a list of all sources and systems that are available</li>
</ul>
This tool has a comprehensive list of command line parameters that can be used to do the above and much more.

<b>Headerer</b>
<p/>
A simple auxilary tool that detects and removes headers and also restores headers for the following systems:
<ul>
	<li>Atari 7800</li>
	<li>Atari Lynx</li>
	<li>Commodore 64 PSID music</li>
	<li>NEC PC-Engine / TurboGrafx 16</li>
	<li>Nintendo Famicom Disk System</li>
	<li>Nintendo Entertainment System / Famicom</li>
	<li>Super Nintendo Entertainment System / Super Famicom</li>
	<li>Super Nintendo Entertainment System / Super Famicom SPC music</li>
</ul>
The saved headers are currently stored in a database; multiple headers allowed per file.

<h3>Power User Tools</h3>

This section is for tools that have been requested for a highly specific purpose and are not generally going to be used by regular users. All of them have a possibility of being included in the main DATabase program eventually.

<b>There are no standalone tools that are for power users at this time.</b>

<h3>Licensing</h3>
<p/>
The preceeding programs use, in part or in whole, code, libraries, and/or applications from the <a href="www.7-zip.org">7-zip project</a>. 7-zip is licenced under the GNU LGPL.<br/>
All other external code is marked as such within the source and correctly attributed to the site and/or person(s) that originally wrote the code.
All code written by project members is licensed under GPL v3. See LICENSE for more details.

<h3>Contributions</h3>

Any feature suggestions or code contributions are accepted. Please take the time to look over the existing code to see proper coding and documentation patterns.