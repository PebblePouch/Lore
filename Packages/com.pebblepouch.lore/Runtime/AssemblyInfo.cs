using System.Runtime.CompilerServices;

// Data changes go through the Lore editor (services, LoreChangeTracker); games only read.
[assembly: InternalsVisibleTo("PebblePouch.Lore.Editor")]
