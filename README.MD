<h2>SabreTools</h2>

<h3>Introduction</h3>

This project started as a desktop port of some core features of Wizard of DATz Redux, but it has since grown into a fully-featured DAT management tool. Below is a brief description of the features available in the suite. For more information about each feature, see the help text in each of the programs. For more information about the parent project, see <a href="https://github.com/mnadareski/wizzardRedux">here</a>.

<h3>Features</h3>

For the most complete set of information, see the <a href="https://raw.githubusercontent.com/mnadareski/wizzardDesktop/master/SabreTools.Helper/README.1ST">README.1ST</a> file. Currently, the SabreTools suite consists of the following programs:

<!--
<b>SabreTools UI</b>
<p/>
A bare-bones attempt at providing a true GUI experience for the SabreTools suite. Currently only provides the following functions:
<ul>
	<li>Importing and Generating DAT files in ClrMamePro and XML formats</li>
</ul>
This tool is not recommended for any usage.
-->

<b>SabreTools</b>
<p/>
The main tool of the SabreTools suite. Performs the majority of the core features of the parent project, including the following (sorted by the original standalone executable, if any):
<ul>
	<li><i>Convert/DATToMiss</i>: Converting DATs from any format to ClrMamePro, Logiqx XML, SabreDAT XML, and to missfile (last part requested by Obiwantje)</li>
	<li><i>DATabase/DATabaseTwo</i>: Importing and Generating DAT files in ClrMamePro and XML formats
		<ul>
			<li>Add and remove sources and systems from the database</li>
			<li>Retrieve a list of all sources and systems that are available</li>
		</ul>
	</li>
	<li><i>DATFromDir/DATFromDirParallel</i>: Create a DAT from a file, a folder, or a set of either</li>
	<li><i>DatSplit</i>: Split a DAT using two different file extensions within the DAT</li>
	<li><i>Filter</i>: Filter a DAT by any criteria that the user chooses including wildcard searches (requested by Obiwantje and others)</li>
	<li><i>HashSplit</i>: Split a DAT or folder of DATs by best available hash or nodump (requested by Obiwantje)</li>
	<li><i>Headerer</i>: Detect and remove copier headers while backing them up or restore headers for the following systems:
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
	</li>
	<li><i>MergeDAT</i>: Merge, diff, and dedupe an arbitrary number of DAT files (requested by Obiwantje and Kludge)
		<ul>
			<li>Read in from commandline, drag-and-drop, a directory, or any combination</li>
			<li>Roms are compared with all information available, so duplicates can exist in the output</li>
			<li>Output type automatically determined by the first input file</li>
			<li>Optionally create a DiffDat of the inputted files instead of a straight merge</li>
			<li>Optionally merge the output DAT so no partial duplicates are included</li>
		</ul>
	</li>
	<li><i>SingleGame</i>: Trim DAT entries and optionally merge into a single game (requested by Kludge)</li>
	<li><i>UncompressedSize</i>: Get various statistics on a DAT or folder of DATs including number of roms, disks, and even total size in a human readable format (requested by Obiwantje and others)</li>
</ul>
This tool has a comprehensive list of command line parameters that can be used to do the above and much more.

<b>SimpleSort</b>
<p/>
A simple rebuild-from-DAT tool that allows users to sort from the commandline.
<ul>
	<li>Rebuild from multiple sources</li>
	<li>Single DAT and output directory support</li>
	<li>Header skipper support</li>
	<li>Verify a folder against a DAT (create a fixDat)</li>
	<li>Rebuild to TGZ archives</li>
	<li>Rebuild to Romba depot</li>
</ul>

<h3>Power User Tools</h3>

This section is for tools that have been requested for a highly specific purpose and are not generally going to be used by regular users. All of them have a possibility of being included in the main DATabase program eventually.

<!-- <b>There are no standalone tools that are for power users at this time.</b> -->

<b>RombaSharp</b>
<p/>
An in-progress tool that will try to act as a C# port of the Go-based Romba program. All features that are not already a part of SabreTools will be attempted to be added to this program. It is NOT ready for use yet.

<b>TGZConvert</b>
<p/>
A simple tool to convert a folder to TGZ, optionally to a specific folder.
<ul>
	<li>Optionally delete source files (if possible)</li>
	<li>Create a Romba depot from an input folder</li>
</ul>

<h3>Licensing</h3>
<p/>
The preceeding programs use, in part or in whole, code, libraries, and/or applications from the <a href="www.7-zip.org">7-zip project</a>. 7-zip is licenced under the GNU LGPL.<br/>
All other external code is marked as such within the source and correctly attributed to the site and/or person(s) that originally wrote the code.
All code written by project members is licensed under GPL v3. See LICENSE for more details.

<h3>Contributions</h3>

Any feature suggestions or code contributions are accepted. Please take the time to look over the existing code to see proper coding and documentation patterns.